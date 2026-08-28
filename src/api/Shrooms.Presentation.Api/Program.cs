using Hangfire;
using Hangfire.SqlServer;
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
using Shrooms.Presentation.Common.Hubs;
using Shrooms.Presentation.Api.Caching;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

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

// ASP.NET Core Identity (provides UserManager, RoleManager infra)
builder.Services.AddIdentityCore<ApplicationUser>(opts =>
{
    opts.Password.RequireDigit = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequiredLength = 6;
    opts.SignIn.RequireConfirmedEmail = false;
})
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ShroomsDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = builder.Configuration["JwtSecret"] ?? "default-secret-key-change-in-production-min32chars!!";
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
        ValidateIssuer = false,
        ValidateAudience = false,
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
    });
}

// CORS
var corsOrigins = builder.Configuration["CorsOrigins"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (string.IsNullOrEmpty(corsOrigins) || corsOrigins == "*")
        {
            // AllowAnyOrigin() cannot be combined with AllowCredentials() per CORS spec.
            // SetIsOriginAllowed echoes the actual request origin, satisfying withCredentials.
            policy.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials()
                  .WithExposedHeaders("Content-Disposition");
        }
        else
        {
            policy.WithOrigins(corsOrigins.Split(';'))
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .WithExposedHeaders("Content-Disposition");
        }
    });
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
.RemoveProcessor<ResizeWebProcessor>()
.AddProcessor<ClampingResizeWebProcessor>();

if (builder.Configuration.GetValue<bool>("ImageSharp:DisableCache"))
{
    builder.Services.AddSingleton<IImageCache, NullImageCache>();
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

app.UseAuthentication();
app.UseMiddleware<MultiTenancyMiddleware>();
app.UseMiddleware<ImageResizerMiddleware>();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHangfireDashboard();

app.MapControllers();
app.MapHub<NotificationHub>("/signalr");
app.MapHealthChecks("/healthz");
app.MapEmailPreview();

// Serve uploaded pictures via the configured IStorage so the same provider that handles
// uploads also handles reads (local FS in dev, Azure Blob in staging/prod). Browser <img>
// tags don't send JWT, so this endpoint is anonymous — GUID filenames make URLs unguessable.
var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
app.MapGet("/storage/{tenant}/{filename}", async (string tenant, string filename, Shrooms.Infrastructure.Storage.IStorage storage) =>
{
    var stream = await storage.GetPictureAsync(filename, tenant.ToLowerInvariant());
    if (stream == null)
    {
        return Results.NotFound();
    }

    if (!contentTypeProvider.TryGetContentType(filename, out var contentType))
    {
        contentType = "application/octet-stream";
    }

    return Results.File(stream, contentType);
}).AllowAnonymous();

app.Run();
