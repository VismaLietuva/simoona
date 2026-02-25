using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Constants;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.IoC;
using Shrooms.Presentation.Api.Middlewares;
using Shrooms.Presentation.Common.Hubs;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
    return new ShroomsDbContext(optionsBuilder.Options);
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
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/signalr"))
            {
                context.Token = accessToken;
            }
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
        if (string.IsNullOrEmpty(corsOrigins))
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
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
builder.Services.AddSwaggerGen();

// Application services (IoC bootstrapper)
builder.Services.AddShrooms();

var app = builder.Build();

// Apply pending EF Core migrations at startup.
// For existing databases (migrated from EF6), the InitialBaseline migration is skipped
// by marking it as already applied — it only needs to run on fresh installs.
using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var connStr = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    var options = new DbContextOptionsBuilder<ShroomsDbContext>()
        .UseSqlServer(connStr)
        .Options;
    using var db = new ShroomsDbContext(options);
    ApplyMigrationsForBrownfieldDatabase(db);
}

static void ApplyMigrationsForBrownfieldDatabase(ShroomsDbContext db)
{
    // Check whether __EFMigrationsHistory exists
    var historyTableCount = db.Database.SqlQueryRaw<int>(
        "SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory'")
        .First();

    if (historyTableCount == 0)
    {
        // No EF Core history table → this is either a fresh DB or a brownfield EF6 DB.
        // Check if AspNetUsers already exists (brownfield indicator).
        var usersTableCount = db.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers'")
            .First();

        if (usersTableCount > 0)
        {
            // Brownfield EF6 database: create history table and mark InitialBaseline applied
            // so EF Core doesn't try to re-create all existing tables.
            db.Database.ExecuteSqlRaw(
                "CREATE TABLE [__EFMigrationsHistory] (" +
                "[MigrationId] nvarchar(150) NOT NULL, " +
                "[ProductVersion] nvarchar(32) NOT NULL, " +
                "CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId]))");

            var baselineMigration = db.Database.GetPendingMigrations()
                .FirstOrDefault(m => m.EndsWith("InitialBaseline"));
            if (baselineMigration != null)
            {
                db.Database.ExecuteSqlRaw(
                    $"INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('{baselineMigration}', '10.0.0')");
            }
        }
    }

    // Apply any remaining pending migrations (e.g. AddIdentityV3Columns on brownfield DBs)
    db.Database.Migrate();
}

// Middleware pipeline
app.UseRouting();
app.UseCors();
app.UseMiddleware<MultiTenancyMiddleware>();
app.UseMiddleware<ImageResizerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHangfireDashboard();

app.MapControllers();
app.MapHub<NotificationHub>("/signalr");

app.Run();
