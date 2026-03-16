var builder = DistributedApplication.CreateBuilder(args);

// Admin credentials for the initial DB seed – defaults come from
// appsettings.Development.json; override via user secrets or env vars.
var adminEmail    = builder.AddParameter("admin-email");
var adminPassword = builder.AddParameter("admin-password", secret: true);

var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sqlServer = builder.AddSqlServer("sqlserver", password: sqlPassword, port: 1434)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint("tcp", e => e.IsProxied = false);

// Resource names match the connection string keys in appsettings.json so Aspire
// injects ConnectionStrings__DefaultConnection and ConnectionStrings__BackgroundJobs.
var simoonaDb = sqlServer.AddDatabase("DefaultConnection", "SimoonaDB");
var jobsDb    = sqlServer.AddDatabase("BackgroundJobs",   "SimoonaDBJobs");

var api = builder.AddProject<Projects.Shrooms_Presentation_Api>("api")
    .WithHttpEndpoint(port: 50321, name: "http")
    .WithReference(simoonaDb)
    .WithReference(jobsDb)
    .WaitFor(simoonaDb);

// Run build/setup.ps1 after the API has started (migrations already applied).
// ConnectionStrings__DefaultConnection is injected by WithReference(simoonaDb).
// Admin credentials come from the admin-email / admin-password parameters.
builder.AddExecutable("db-setup", "pwsh", "../../build",
    "-ExecutionPolicy", "Bypass",
    "-File", "setup.ps1",
    "-Email", adminEmail,
    "-Password", adminPassword)
    .WithReference(simoonaDb)
    .WaitFor(api);

// webapp: runs "npm run start:aspire" which gulp-builds then starts the Express server.
// The webapp's gulp.config.js has endpoint hardcoded to http://localhost:50321, so no
// extra env config is needed as long as the API port is fixed above.
builder.AddJavaScriptApp("webapp", "../../webapp", "start:aspire")
    .WithHttpEndpoint(port: 3000, name: "http", env: "PORT")
    .WithEnvironment("NODE_ENV", "build")
    .WaitFor(api);

builder.Build().Run();
