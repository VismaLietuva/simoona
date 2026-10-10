using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.IoC;
using Shrooms.Presentation.Api.BackgroundWorkers;
using Shrooms.Presentation.Api.Endpoints;
using Shrooms.Presentation.Api.Middlewares;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.Common.Hubs;
using Shrooms.Presentation.Api.Caching;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.Api.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = false;
});

// DbContext with per-request tenant-aware connection string
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ShroomsDbContext>(sp =>
{
    var httpContext = sp.GetService<IHttpContextAccessor>()?.HttpContext;
    var tenantName = httpContext?.Items["tenantName"] as string;
    // Fallback: check ITenantNameContainer for background tasks (AsyncRunner)
    if (string.IsNullOrEmpty(tenantName))
    {
        tenantName = sp.GetService<ITenantNameContainer>()?.TenantName;
    }
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connStr = !string.IsNullOrEmpty(tenantName)
        ? configuration.GetConnectionString(tenantName)
        : null;
    connStr ??= configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    var optionsBuilder = new DbContextOptionsBuilder<ShroomsDbContext>();
    optionsBuilder.UseSqlServer(connStr);
    optionsBuilder.ConfigureWarnings(w =>
    {
        w.Ignore(RelationalEventId.PendingModelChangesWarning);
        w.Log(CoreEventId.InvalidIncludePathError);
    });
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    return new ShroomsDbContext(optionsBuilder.Options, httpContextAccessor)
    {
        ConnectionName = tenantName
    };
});
builder.Services.AddScoped<IDbContext>(sp => sp.GetRequiredService<ShroomsDbContext>());

// Output caching for the widget endpoints. Their responses are shaped by organization and
// permission set rather than by user, so PermissionAwareCacheOutputFilterAttribute keys entries
// on those and writes evict them by tag through IWidgetCacheInvalidator.
builder.Services.AddOutputCache();
builder.Services.AddScoped<IWidgetCacheInvalidator, WidgetCacheInvalidator>();

builder.Services.AddSingleton<Shrooms.Presentation.Api.Helpers.IReturnUrlValidator>(sp =>
    new Shrooms.Presentation.Api.Helpers.ReturnUrlValidator(sp.GetRequiredService<IConfiguration>()));

// ASP.NET Core Identity (provides UserManager, RoleManager infra)
builder.Services.AddIdentityCore<ApplicationUser>(opts =>
{
    // Mirrors the Next.js password schema (src/lib/password-schema.ts); the server is the authority.
    opts.Password.RequireDigit = true;
    opts.Password.RequireLowercase = true;
    opts.Password.RequireUppercase = true;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequiredLength = 8;
    opts.SignIn.RequireConfirmedEmail = false;

    // Five wrong passwords lock the account for fifteen minutes.
    opts.Lockout.AllowedForNewUsers = true;
    opts.Lockout.MaxFailedAccessAttempts = 5;
    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ShroomsDbContext>()
    .AddDefaultTokenProviders();

// No fallback key: a deployment without JwtSecret must fail to start. Issuer and audience are validated.
var jwtKey = builder.Configuration["JwtSecret"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("JwtSecret must be configured and at least 32 bytes long.");
}
var isLocalEnvironment = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Docker");
if (!isLocalEnvironment && jwtKey.StartsWith("your-secret-key", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("JwtSecret is still the sample value from appsettings.json; set a real secret outside local development.");
}
var jwtIssuer = builder.Configuration["JwtIssuer"] ?? Shrooms.Domain.Services.Jwt.JwtTokenService.DefaultIssuer;
var jwtAudience = builder.Configuration["JwtAudience"] ?? Shrooms.Domain.Services.Jwt.JwtTokenService.DefaultAudience;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    // Allow token from query string for SignalR
    // Old client sends "token"; new @microsoft/signalr client sends "access_token"
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var path = context.HttpContext.Request.Path;
            if (path.StartsWithSegments("/signalr"))
            {
                var accessToken = context.Request.Query["access_token"].ToString();
                if (string.IsNullOrEmpty(accessToken))
                    accessToken = context.Request.Query["token"].ToString();
                if (!string.IsNullOrEmpty(accessToken))
                    context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(
                "JWT auth failed for {Path} from {Ip}: {Type} — {Message}",
                context.Request.Path,
                context.HttpContext.Connection.RemoteIpAddress,
                context.Exception.GetType().Name,
                context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});

// Deny by default: every endpoint needs an authenticated user unless it opts out with [AllowAnonymous].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// External cookie used to round-trip the identity returned by social IdPs back to /Account/ExternalLoginCallback.
// Required because every social handler below uses IdentityConstants.ExternalScheme as its SignInScheme.
var externalSchemeRegistered = false;
void EnsureExternalCookie()
{
    if (externalSchemeRegistered) return;
    builder.Services.AddAuthentication().AddCookie(IdentityConstants.ExternalScheme, o =>
    {
        o.Cookie.Name = IdentityConstants.ExternalScheme;
        o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    });
    externalSchemeRegistered = true;
}

// Social auth (optional, reads from config)
if (!string.IsNullOrEmpty(builder.Configuration["GoogleAccountClientId"]))
{
    EnsureExternalCookie();
    builder.Services.AddAuthentication().AddGoogle(opts =>
    {
        opts.ClientId = builder.Configuration["GoogleAccountClientId"];
        opts.ClientSecret = builder.Configuration["GoogleAccountClientSecret"];
        opts.SignInScheme = IdentityConstants.ExternalScheme;
        // ExternalEmailTrust reads "email_verified" (v3 userinfo); v2 named it "verified_email". Both mapped.
        opts.ClaimActions.MapJsonKey("email_verified", "verified_email");
        opts.ClaimActions.MapJsonKey("email_verified", "email_verified");
    });
}
if (!string.IsNullOrEmpty(builder.Configuration["FacebookAccountAppId"]))
{
    EnsureExternalCookie();
    builder.Services.AddAuthentication().AddFacebook(opts =>
    {
        opts.AppId = builder.Configuration["FacebookAccountAppId"];
        opts.AppSecret = builder.Configuration["FacebookAccountAppSecret"];
        opts.SignInScheme = IdentityConstants.ExternalScheme;
    });
}
if (!string.IsNullOrEmpty(builder.Configuration["MicrosoftAccountClientId"]))
{
    EnsureExternalCookie();
    builder.Services.AddAuthentication().AddMicrosoftAccount(opts =>
    {
        opts.ClientId = builder.Configuration["MicrosoftAccountClientId"];
        opts.ClientSecret = builder.Configuration["MicrosoftAccountClientSecret"];
        opts.SignInScheme = IdentityConstants.ExternalScheme;
        // The email is set by the account's own Entra tenant: request an id_token and keep its "tid".
        opts.Scope.Add("openid");
        opts.Events.OnCreatingTicket = context =>
        {
            var tenantId = ExternalEmailTrust.ReadMicrosoftTenantId(context.TokenResponse.Response.RootElement);
            if (!string.IsNullOrEmpty(tenantId))
            {
                context.Identity.AddClaim(new System.Security.Claims.Claim(ExternalEmailTrust.MicrosoftTenantClaim, tenantId));
            }

            return Task.CompletedTask;
        };
    });
    if (string.IsNullOrWhiteSpace(builder.Configuration[ExternalEmailTrust.TrustedTenantsSetting]))
    {
        Console.WriteLine($"WARNING: Microsoft sign-in is configured but {ExternalEmailTrust.TrustedTenantsSetting} is empty. Microsoft emails are then never trusted, so Microsoft can neither register nor link accounts; set it to the Entra tenant id(s) whose addresses are administrator-controlled.");
    }
}

builder.Services.AddSingleton<IExternalEmailTrust>(_ => ExternalEmailTrust.FromConfiguration(builder.Configuration));

// CORS
var corsOrigins = builder.Configuration["CorsOrigins"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (string.IsNullOrEmpty(corsOrigins) || corsOrigins == "*")
        {
            // Reflecting any origin with credentials is only acceptable in local development.
            if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Docker"))
            {
                throw new InvalidOperationException(
                    "CorsOrigins must list the allowed client origins (semicolon-separated) outside Development; '*' is not permitted.");
            }

            // AllowAnyOrigin() cannot be combined with AllowCredentials() per CORS spec.
            // SetIsOriginAllowed echoes the actual request origin, satisfying withCredentials.
            policy.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials()
                  .WithExposedHeaders("Content-Disposition");
        }
        else
        {
            policy.WithOrigins(corsOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .WithExposedHeaders("Content-Disposition");
        }
    });
});

var authRequestsPerMinute = int.TryParse(builder.Configuration["AuthRateLimitPerMinute"], out var authLimit) && authLimit > 0
    ? authLimit
    : 10;
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\":\"too_many_requests\",\"error_description\":\"Too many attempts. Try again in a minute.\"}",
            cancellationToken);
    };
    // The Next.js server calls these for every browser: the forwarded address is used only with the shared secret.
    var trustedClientIpSecret = builder.Configuration["TrustedClientIpSecret"];
    options.AddPolicy(AuthRateLimit.PolicyName, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            AuthRateLimit.PartitionKey(httpContext, trustedClientIpSecret),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authRequestsPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// SignalR (in-box with Sdk.Web)
builder.Services.AddSignalR();

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString(DataLayerConstants.ConnectionStringNameBackgroundJobs)
            ?? builder.Configuration.GetConnectionString("DefaultConnection")
            ?? string.Empty,
        new SqlServerStorageOptions
        {
            QueuePollInterval = TimeSpan.FromSeconds(
                int.TryParse(builder.Configuration["BackgroundWorkerSqlPollingIntervalInSeconds"], out var interval)
                    ? interval
                    : 15)
        }));
builder.Services.AddHangfireServer();

// MVC + API controllers
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Converters.Add(new Shrooms.Presentation.Api.Helpers.EmptyToNullConverter());
        options.SerializerSettings.Converters.Add(new Shrooms.Presentation.Api.Helpers.FormattedDecimalConverter());
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Skip actions where Swashbuckle cannot determine the HTTP method.
    // This happens when controllers inherit virtual actions from a base class
    // and override them without re-applying the [HttpGet/Post/Put/Delete] attribute
    // (C# does not inherit method attributes through overrides).
    c.DocInclusionPredicate((_, api) => api.HttpMethod != null);
    // When two actions produce the same route+verb (e.g. base and override both visible),
    // pick the first one instead of throwing.
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

    // JWT bearer auth — adds the "Authorize" button at the top of Swagger UI.
    // Paste the raw token (no "Bearer " prefix) and it is sent on every request.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste your JWT access token below (without the 'Bearer ' prefix).",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.SchemaFilter<Shrooms.Presentation.Api.Helpers.StringEnumSchemaFilter>();
});

builder.Services.AddSwaggerGenNewtonsoftSupport();

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Application services (IoC bootstrapper)
builder.Services.AddShrooms();
builder.Services.AddTransient<PostNotifier>();
builder.Services.AddTransient<CommentNotifier>();

// ImageSharp.Web: on-the-fly image resizing for /storage/* URLs that carry
// width/height/mode query commands. Source images are read via IStorage (local FS in dev,
// Azure Blob in staging/prod) through our custom StorageImageProvider. Resized variants
// are cached on the App Service's local disk under <ContentRoot>/storage-cache so the
// resize work happens at most once per (URL + commands) until the app instance restarts.
// 'mode=max|crop|pad|stretch' (master/ImageResizer.NET convention) is aliased to the
// ImageSharp.Web command name 'rmode' so the existing frontend URLs keep working without
// any client-side change.
builder.Services.AddImageSharp(options =>
{
    var defaultOnParse = options.OnParseCommandsAsync;
    options.OnParseCommandsAsync = async context =>
    {
        if (context.Commands.TryGetValue("mode", out var mode) && !context.Commands.Contains("rmode"))
        {
            context.Commands.Remove("mode");
            context.Commands.Add("rmode", mode);
        }

        // Anonymous endpoint: bound the requested dimensions and the number of cache variants.
        ResizeCommandGuard.Clamp(context.Commands);
        if (defaultOnParse != null)
        {
            await defaultOnParse(context);
        }
    };
})
.Configure<PhysicalFileSystemCacheOptions>(options =>
{
    // Simoona is an API-only project with no wwwroot, so PhysicalFileSystemCache's
    // default of resolving CacheFolder against WebRootPath throws at startup. Pin
    // CacheRootPath to ContentRootPath instead, so the cache lives next to the
    // deployed binaries at <ContentRoot>/storage-cache/.
    options.CacheRootPath = builder.Environment.ContentRootPath;
    options.CacheFolder = "storage-cache";
})
.ClearProviders()
.AddProvider<StorageImageProvider>()
// Only resizing is exposed; format/quality/background commands would multiply cache variants.
.RemoveProcessor<ResizeWebProcessor>()
.RemoveProcessor<FormatWebProcessor>()
.RemoveProcessor<QualityWebProcessor>()
.RemoveProcessor<BackgroundColorWebProcessor>()
.AddProcessor<ClampingResizeWebProcessor>();

// Bound a single decode (a small PNG can declare a 40000x40000 canvas); covers images stored before the upload check.
SixLabors.ImageSharp.Configuration.Default.MemoryAllocator = SixLabors.ImageSharp.Memory.MemoryAllocator.Create(
    new SixLabors.ImageSharp.Memory.MemoryAllocatorOptions { AllocationLimitMegabytes = 256 });

if (builder.Configuration.GetValue<bool>("ImageSharp:DisableCache"))
{
    builder.Services.AddSingleton<IImageCache, NullImageCache>();
}

// Refuse to start outside local development when Basic credentials are missing or still the sample values,
// or when TrustedClientIpSecret is missing (every user would share one rate-limit budget).
if (!isLocalEnvironment && string.IsNullOrWhiteSpace(builder.Configuration["TrustedClientIpSecret"]))
{
    throw new InvalidOperationException(
        "TrustedClientIpSecret must be configured (and API_CLIENT_IP_SECRET on the web client) so sign-in rate limits are counted per browser, not per server.");
}

if (!isLocalEnvironment)
{
    var basicUsername = builder.Configuration["BasicUsername"];
    var basicPassword = builder.Configuration["BasicPassword"];
    if (string.IsNullOrWhiteSpace(basicUsername) || string.IsNullOrWhiteSpace(basicPassword)
        || basicUsername == "basicUsername" || basicPassword == "basicPassword")
    {
        throw new InvalidOperationException(
            "BasicUsername and BasicPassword must be set to non-default values outside local development. They protect the external job endpoints.");
    }
}

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var tenants = configuration.GetSection("Organizations").GetChildren()
        .Select(c => c.Key)
        .ToList();

    foreach (var tenant in tenants)
    {
        var connStr = configuration.GetConnectionString(tenant);
        if (string.IsNullOrWhiteSpace(connStr) || connStr.StartsWith("$("))
        {
            logger.LogInformation("Skipping migration for tenant '{Tenant}' — no connection string configured.", tenant);
            continue;
        }

        try
        {
            _ = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connStr);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Skipping migration for tenant '{Tenant}' — connection string is malformed.", tenant);
            continue;
        }

        logger.LogInformation("Migrating tenant '{Tenant}'.", tenant);
        var options = new DbContextOptionsBuilder<ShroomsDbContext>()
            .UseSqlServer(connStr)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        using var db = new ShroomsDbContext(options);
        db.Database.Migrate();
    }
}

// Middleware pipeline
// Opt-in: trusting X-Forwarded-* from any peer lets a client that reaches Kestrel directly spoof its address.
// Only the right-most entry is read (ForwardLimit = 1); list the proxies in ForwardedHeadersKnownProxies when known.
var trustForwardedHeaders = builder.Configuration.GetValue<bool?>("TrustForwardedHeaders") ?? false;
var platformForwardedHeaders = string.Equals(builder.Configuration["ASPNETCORE_FORWARDEDHEADERS_ENABLED"], "true", StringComparison.OrdinalIgnoreCase);
if (trustForwardedHeaders && platformForwardedHeaders)
{
    // The platform already runs the forwarded-headers middleware; running it twice would read a client-controlled entry.
    app.Logger.LogInformation("Forwarded headers are handled by the platform (ASPNETCORE_FORWARDEDHEADERS_ENABLED); skipping the application's own middleware.");
}
else if (trustForwardedHeaders)
{
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        ForwardLimit = 1
    };
    forwardedHeadersOptions.KnownNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    foreach (var proxy in (builder.Configuration["ForwardedHeadersKnownProxies"] ?? string.Empty)
                 .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (System.Net.IPAddress.TryParse(proxy, out var proxyAddress))
        {
            forwardedHeadersOptions.KnownProxies.Add(proxyAddress);
        }
    }

    if (forwardedHeadersOptions.KnownProxies.Count == 0)
    {
        // Empty lists believe every immediate peer: fine on App Service (reachable only via the front end), not on a bare host.
        app.Logger.LogWarning("TrustForwardedHeaders is on without ForwardedHeadersKnownProxies: X-Forwarded-* is trusted from every immediate peer. Safe only when the API is reachable solely through the platform's reverse proxy; otherwise set ForwardedHeadersKnownProxies.");
    }

    app.UseForwardedHeaders(forwardedHeadersOptions);
}

// Behind a TLS-terminating proxy this needs TrustForwardedHeaders too, or redirects loop.
var enforceHttps = builder.Configuration.GetValue<bool?>("EnforceHttps") ?? false;
if (enforceHttps)
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        // Locked-down CSP for JSON and images; HTML documents (Swagger, Hangfire) keep their own scripts.
        var contentType = context.Response.ContentType ?? string.Empty;
        if (!contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
        {
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        }

        return Task.CompletedTask;
    });

    await next();
});

// Normalize double-slash paths (e.g. //Account/Foo → /Account/Foo) sent by the SPA
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path != null && path.StartsWith("//"))
    {
        context.Request.Path = "/" + path.TrimStart('/');
    }
    await next();
});

// Serve the SPA bundle from wwwroot at the site root. Registered before UsePathBase so these
// only match root-relative asset paths and never intercept anything under /api. Only the Linux
// deploy bundles a wwwroot; on Windows the API is an IIS virtual application under /api and IIS
// serves the SPA from the site root, so guard on its presence rather than relying on the
// behaviour of a null WebRootPath.
var spaRootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var spaIndexPath = Path.Combine(spaRootPath, "index.html");
if (Directory.Exists(spaRootPath))
{
    app.Use(async (context, next) =>
    {
        var requestPath = context.Request.Path;
        if (!context.Request.PathBase.HasValue
            && !requestPath.StartsWithSegments("/api")
            && !Path.HasExtension(requestPath.Value ?? string.Empty)
            && File.Exists(spaIndexPath))
        {
            context.Request.Path = "/index.html";
        }

        await next();
    });

    app.UseDefaultFiles();
    app.UseStaticFiles();
}

// The API used to be an IIS virtual application mounted at /api, which supplied that prefix
// for free. Linux App Service has no virtual applications, so add it explicitly: controllers
// are routed at the root ([Route("Account")]) and the SPA calls /api/*.
app.UsePathBase("/api");

app.UseImageSharp();

app.UseRouting();
app.UseCors();
app.UseRateLimiter();

// Development only; before UseAuthorization so the fallback policy does not block the UI.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseMiddleware<MultiTenancyMiddleware>();
app.UseMiddleware<ImageResizerMiddleware>();
app.UseAuthorization();

// After UseAuthorization on purpose: a cache hit short-circuits the rest of the pipeline, so
// placing this earlier would serve stored responses without running the endpoint.s authorization.
app.UseOutputCache();

// Browser page without a usable bearer-token flow: Development only, local requests.
if (app.Environment.IsDevelopment())
{
    app.MapHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new IDashboardAuthorizationFilter[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
    }).AllowAnonymous();
}

app.MapControllers();
app.MapHub<NotificationHub>("/signalr");
app.MapHealthChecks("/healthz").AllowAnonymous();
app.MapEmailPreview();

// Serve uploaded pictures via the configured IStorage so the same provider that handles
// uploads also handles reads (local FS in dev, Azure Blob in staging/prod). Browser <img>
// tags don't send JWT, so this endpoint is anonymous — GUID filenames make URLs unguessable.
var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
app.MapGet("/storage/{tenant}/{filename}", async (string tenant, string filename, Shrooms.Infrastructure.Storage.IStorage storage, HttpContext httpContext) =>
{
    // Route values are URL-decoded ("..%5C..%5Cappsettings.json" would be a traversal on Windows); only bare
    // image file names are served, anything else is treated as missing.
    if (!Shrooms.Infrastructure.Storage.BlobKeyGuard.IsSafeBlobKey(filename)
        || !Shrooms.Infrastructure.Storage.BlobKeyGuard.IsSafeContainer(tenant)
        || !Shrooms.Infrastructure.Storage.BlobKeyGuard.HasAllowedImageExtension(filename))
    {
        return Results.NotFound();
    }

    var stream = await storage.GetPictureAsync(filename, tenant.ToLowerInvariant());
    if (stream == null)
    {
        return Results.NotFound();
    }

    if (!contentTypeProvider.TryGetContentType(filename, out var contentType))
    {
        contentType = "application/octet-stream";
    }

    httpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";
    return Results.File(stream, contentType);
}).AllowAnonymous();

app.Run();
