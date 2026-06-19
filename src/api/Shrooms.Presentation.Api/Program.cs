using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.IoC;
using Shrooms.Presentation.Api.BackgroundWorkers;
using Shrooms.Presentation.Api.Middlewares;
using Shrooms.Presentation.Common.Hubs;
using Shrooms.Presentation.Api.Caching;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
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
    return new ShroomsDbContext(optionsBuilder.Options, httpContextAccessor);
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
        ClockSkew = TimeSpan.Zero
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
            logger.LogError(context.Exception, "JWT authentication failed for {Path}", context.Request.Path);
            return Task.CompletedTask;
        }
    };
});

// Social auth (optional, reads from config)
if (!string.IsNullOrEmpty(builder.Configuration["GoogleAccountClientId"]))
{
    builder.Services.AddAuthentication().AddGoogle(opts =>
    {
        opts.ClientId = builder.Configuration["GoogleAccountClientId"];
        opts.ClientSecret = builder.Configuration["GoogleAccountClientSecret"];
    });
}
if (!string.IsNullOrEmpty(builder.Configuration["FacebookAccountAppId"]))
{
    builder.Services.AddAuthentication().AddFacebook(opts =>
    {
        opts.AppId = builder.Configuration["FacebookAccountAppId"];
        opts.AppSecret = builder.Configuration["FacebookAccountAppSecret"];
    });
}
if (!string.IsNullOrEmpty(builder.Configuration["MicrosoftAccountClientId"]))
{
    builder.Services.AddAuthentication().AddMicrosoftAccount(opts =>
    {
        opts.ClientId = builder.Configuration["MicrosoftAccountClientId"];
        opts.ClientSecret = builder.Configuration["MicrosoftAccountClientSecret"];
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
            policy.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        }
        else
        {
            policy.WithOrigins(corsOrigins.Split(';'))
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
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
});

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
.AddProvider<StorageImageProvider>();

if (builder.Configuration.GetValue<bool>("ImageSharp:DisableCache"))
{
    builder.Services.AddSingleton<IImageCache, NullImageCache>();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var connStr = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    var options = new DbContextOptionsBuilder<ShroomsDbContext>()
        .UseSqlServer(connStr)
        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
        .Options;
    using var db = new ShroomsDbContext(options);

    // TODO: Remove this block once all environments have been migrated to .NET 10.
    // Existing databases were created by the old .NET Framework app and have no EF Core
    // migration history. Without this, Migrate() tries to recreate all tables and fails.
    var conn = db.Database.GetDbConnection();
    conn.Open();
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory')
            BEGIN
                CREATE TABLE __EFMigrationsHistory (
                    MigrationId NVARCHAR(150) NOT NULL,
                    ProductVersion NVARCHAR(32) NOT NULL,
                    CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
                )
            END
            IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260225115301_InitialBaseline')
            BEGIN
                INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
                VALUES ('20260225115301_InitialBaseline', '10.0.0')
            END
            -- SeedInitialData inserts reference rows (KudosTypes, Roles, Permissions, etc.)
            -- that already exist in brownfield databases. Its INSERT statements reference
            -- columns that SQL Server validates at compile time even inside dead-branch
            -- IF NOT EXISTS blocks, causing startup failures when the old schema is missing
            -- those columns. Brownfield databases are detected by having existing rows in
            -- Organizations; skipping is safe because all seed data is already present.
            IF OBJECT_ID('dbo.Organizations') IS NOT NULL
            BEGIN
                IF EXISTS (SELECT TOP 1 1 FROM dbo.Organizations)
                   AND NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260302000000_SeedInitialData')
                BEGIN
                    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
                    VALUES ('20260302000000_SeedInitialData', '10.0.0')
                END
            END
            DELETE FROM __EFMigrationsHistory
            WHERE MigrationId IN (
                '20260421000002_AddIsDeletedToLotteries',
                '20260421000004_RemoveIsDeletedFromLotteries'
            )";
        cmd.ExecuteNonQuery();
    }

    // TODO: Remove once staging has been patched (RemoveShadowFKColumns already applied there,
    // so the migration fix won't run — this one-time script does the rename directly).
    using (var fixCmd = conn.CreateCommand())
    {
        fixCmd.CommandText = @"
            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Floors' AND COLUMN_NAME = 'Picture_Id')
                EXEC sp_rename 'Floors.Picture_Id', 'PictureId1', 'COLUMN';
            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NotificationsSettings' AND COLUMN_NAME = 'ApplicationUser_Id')
                EXEC sp_rename 'NotificationsSettings.ApplicationUser_Id', 'ApplicationUserId', 'COLUMN';
            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CommitteeSuggestions' AND COLUMN_NAME = 'User_Id')
               AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CommitteeSuggestions' AND COLUMN_NAME = 'UserId')
                EXEC sp_rename 'CommitteeSuggestions.User_Id', 'UserId', 'COLUMN';
            -- Fix NULL Created/Modified in brownfield rows — EF Core reads NULL as DateTime.MinValue
            -- (0001-01-01) which is out of range for SQL Server's datetime type.
            UPDATE AspNetUsers SET Created = GETUTCDATE() WHERE Created IS NULL;
            UPDATE AspNetUsers SET Modified = GETUTCDATE() WHERE Modified IS NULL;";
        fixCmd.ExecuteNonQuery();
    }

    db.Database.Migrate();
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
