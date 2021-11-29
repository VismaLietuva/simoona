#addin "nuget:?package=Cake.FileHelpers&version=3.2.0"
#addin "nuget:?package=Cake.Gulp&version=0.12.0"
#addin "nuget:?package=Cake.Hosts&version=1.5.1"
#addin "nuget:?package=Cake.IIS&version=0.4.2"
#addin "nuget:?package=Cake.Npm&version=0.17.0"
#addin "nuget:?package=Cake.SqlServer&version=2.0.1"
#addin "nuget:?package=Microsoft.Win32.Registry&version=4.5.0"
#addin "nuget:?package=System.Reflection.TypeExtensions&version=4.5.0"

#tool "nuget:?package=EntityFramework&version=6.1.3"
#tool "nuget:?package=vswhere&version=2.6.7"

using System.Data.SqlClient;
using Path = System.IO.Path;

var target = Argument("activity", "Start");
var organization = Argument("organization", "test");
var email = Argument("email", "");
var connectionString = Argument("connectionString", "");
var dbName = Argument("dbName", "SimoonaDb");
var jobsDbName = dbName + "Jobs";
var dropDb = Argument("dropdb", "");
var APIpath = "../src/api/";
var webAppPath = "../src/webapp/";
var webAppHostName = "app.simoona.local";
var applicationPool = "Simoona";
FilePath logFile;

Setup(context =>
{
    CreateDirectory("./logs");
    logFile = new FilePath(String.Format("logs/{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmss")));
});

TaskSetup(setupContext =>
{
    var message = string.Format("Task {0} is starting", setupContext.Task.Name);
    LogMessage(logFile, message);
});

Task("CreateDatabase")
    .Does(() => 
{
    using (var connection = OpenSqlConnection(connectionString))
    {
        DropDatabaseIfNeeded(dropDb, dbName);
        DropDatabaseIfNeeded(dropDb, jobsDbName);

        if(!DatabaseExists(connectionString, dbName))
        {
            SetSqlCommandTimeout(900);
            ExecuteSqlCommand(connection, "CREATE DATABASE [" + dbName + "]");
            ExecuteSqlCommand(connection, "USE \"" + dbName + "\"");

            ExecuteSqlFile(connection, "initial_db.sql");
            ExecuteSqlCommand(connection, "USE \"" + dbName + "\"");

            var userId = Guid.NewGuid();
            ExecuteSqlCommand(connection, "INSERT INTO dbo.AspNetUsers (Id, FirstName, LastName, OrganizationId, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, GoogleEmail, SecurityStamp, IsManagingDirector, EmploymentDate, PasswordHash) VALUES ('"+userId+"', 'Admin', 'Admin', 1, '"+email+"', 1, 0, 0, 0, 0, '"+email+"', '"+email+"', '"+Guid.NewGuid()+"', 1, GETDATE(), 'ACBQTVhgqV9pBCpMO5BpL566dNaC/AG4SeZAV+Z8gStP2E9y/OqnCHKb40gz/izgFA==')");
            ExecuteSqlCommand(connection, "INSERT INTO dbo.WallModerators (WallId, UserId, IsDeleted, Created, Modified) VALUES (1, '"+userId+"', 0, '1900-01-01', '1900-01-01')");
            ExecuteSqlFile(connection, "admin.sql");
        }
        else
        {
            LogMessage(logFile, string.Format("Database {0} already exists", dbName));
        }

        if(!DatabaseExists(connectionString, jobsDbName))
        {
            SetSqlCommandTimeout(900);

            ExecuteSqlCommand(connection, "CREATE DATABASE [" + jobsDbName + "]");
            AlterJobsDb(connection);
            ExecuteSqlCommand(connection, "USE \"" + jobsDbName + "\"");
            ExecuteSqlFile(connection, "background_jobs.sql");
        }
        else 
        {
            LogMessage(logFile, string.Format("Database {0} already exists", jobsDbName));
        }
    }
    LogMessage(logFile, "Task CreateDatabase is finished successfully");
})
.OnError(exception =>
{
    LogMessage(logFile, exception.Message);
    throw exception;
});

Task("ExecuteMigrations")
    .Does(() =>
{
    connectionString = NormalizeConnectionString(connectionString);
    var fullConnectionString = string.Format("{0}Database={1};", connectionString, dbName);

    var migrateExePath = Context.Tools.Resolve("migrate.exe");
    var assemblyBinPath = Path.GetFullPath(Path.Combine(APIpath, @"Shrooms.DataLayer\bin\Debug"));
    var assemblyName = "Shrooms.DataLayer.dll";

    Information("Migrate.exe path: {0}", migrateExePath);
    Information("Assembly bin path: {0}", assemblyBinPath);

    var settings = new ProcessSettings 
    {
        Arguments = new ProcessArgumentBuilder()
            .Append(assemblyName)
            .Append("/startUpDirectory=\"" + assemblyBinPath + "\"")
            .Append("/connectionProviderName:\"System.Data.SqlClient\"")
            .Append("/connectionString:\"" + fullConnectionString + "\"")
            .Append("/verbose")
    };

    StartProcess(migrateExePath, settings);
});

Task("AddOrganization")
    .IsDependentOn("CreateDatabase")
    .Does(() => 
{
    string xmlFile = APIpath + "Shrooms.Presentation.Api/Web.config";
    System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
    xmlDoc.Load(xmlFile);

    var organizationsNode = xmlDoc.SelectSingleNode("configuration/RegisteredOrganizations");
    organizationsNode.RemoveAll();

    var organizationElement = xmlDoc.CreateElement("add");
    organizationElement.SetAttribute("key", organization);
    organizationElement.SetAttribute("value", organization);
    organizationsNode.AppendChild(organizationElement);

    connectionString = NormalizeConnectionString(connectionString);
    var mainConnectionString = xmlDoc.SelectSingleNode("configuration/connectionStrings/add[not(@name = 'BackgroundJobs') and not(@name = 'StorageConnectionString')]");

    mainConnectionString.Attributes["name"].Value = organization;
    mainConnectionString.Attributes["connectionString"].Value = string.Format("{0}Database={1};", connectionString, dbName);

    var jobsConnectionString = xmlDoc.SelectSingleNode("configuration/connectionStrings/add[@name = 'BackgroundJobs']");
    jobsConnectionString.Attributes["connectionString"].Value = string.Format("{0}Database={1};", connectionString, jobsDbName);

    xmlDoc.Save(xmlFile);
    using (var connection = OpenSqlConnection(connectionString))
    {
        ExecuteSqlCommand(connection, "USE \"" + dbName + "\"");
        ExecuteSqlCommand(connection, "UPDATE dbo.Organizations SET Name='" + organization + "', ShortName='" + organization + "', AuthenticationProviders='internal;google;facebook' WHERE Id=1");
    }
    LogMessage(logFile, "Task AddOrganization is finished successfully");
})
.OnError(exception =>
{
    LogMessage(logFile, exception.Message);
    throw exception;
});

Task("Restore")
    .Does(() => 
{
    NuGetRestore(APIpath + "Shrooms.sln");
    LogMessage(logFile, "Task Restore is finished successfully");
})
.OnError(exception =>
{
    LogMessage(logFile, exception.Message);
    throw exception;
});

Task("BuildAPI")
    .IsDependentOn("CreateDatabase")
    .IsDependentOn("AddOrganization")
    .IsDependentOn("Restore")
    .Does(ctx => 
{
    var fileLogger = new MSBuildFileLogger() {
        AppendToLogFile = true,
        LogFile = logFile,
        ShowTimestamp = true,
        Verbosity = Verbosity.Minimal
    };

    var settings = new MSBuildSettings
    {
        Verbosity = Verbosity.Quiet,
        Configuration = "Debug"
    };

    var msbuildPath = GetMSBuildPath(ctx.FileSystem, ctx.Environment, MSBuildPlatform.Automatic);

    if (msbuildPath != null) 
    {
        settings.ToolPath = msbuildPath;
    }

    settings.FileLoggers.Add(fileLogger);

    MSBuild(APIpath + "Shrooms.sln", settings);
    LogMessage(logFile, "Task BuildAPI is finished successfully");
});


Task("BuildWebApp")
    .Does(() => 
{
    NpmInstall(settings => settings.FromPath(webAppPath).WithLogLevel(NpmLogLevel.Error));
    Gulp.Local.Execute(settings => settings.WithArguments("wiredep --silent").WorkingDirectory = webAppPath);
    Gulp.Local.Execute(settings => settings.WithArguments("build-dev --silent").WorkingDirectory = webAppPath);
    LogMessage(logFile, "Task BuildWebApp is finished successfully");
})
.OnError(exception =>
{
    LogMessage(logFile, exception.Message);
    throw exception;
});

Task("CreateApplicationPool")
    .Does(() =>
{
    CreatePool(new ApplicationPoolSettings()
    {
        Name = applicationPool,
        IdentityType = IdentityType.LocalSystem,
        Overwrite = true
    });
    LogMessage(logFile, "Task CreateApplicationPool is finished successfully");
})
.OnError(exception =>
{
    LogMessage(logFile, exception.Message);
    throw exception;
});

Task("CreateAPIWebsite")
    .IsDependentOn("CreateApplicationPool")
    .Does(() =>
{
    CreateWebsite(new WebsiteSettings()
    {
        Name = "SimoonaAPI",
        PhysicalDirectory = APIpath + "Shrooms.Presentation.Api",
        Binding = IISBindings.Http
                    .SetHostName("")
                    .SetIpAddress("*")
                    .SetPort(50321),
        ApplicationPool = new ApplicationPoolSettings()
        {
            Name = applicationPool
        }
    });
    LogMessage(logFile, "Task CreateAPIWebsite is finished successfully");
})
.OnError(exception =>
{
    LogMessage(logFile, exception.Message);
    throw exception;
});

Task("CreateWebAppWebsite")
    .IsDependentOn("CreateApplicationPool")
    .Does(() =>
{
    CreateWebsite(new WebsiteSettings()
    {
        Name = "SimoonaWebApp",
        PhysicalDirectory = webAppPath + "build",
        Binding = IISBindings.Http
                    .SetHostName(webAppHostName)
                    .SetIpAddress("*")
                    .SetPort(80),
        ApplicationPool = new ApplicationPoolSettings()
        {
            Name = applicationPool
        }
    });
    LogMessage(logFile, "Task CreateWebAppWebsite is finished successfully");
})
.OnError(exception =>
{
    LogMessage(logFile, exception.Message);
    throw exception;
});

Task("CreateHostsRecord")
    .Does(() => 
{
    if(!HostsRecordExists("127.0.0.1", webAppHostName)) 
    {
        AddHostsRecord("127.0.0.1", webAppHostName);
    }
    else {
        LogMessage(logFile, "Hosts record already exists");
    }
    LogMessage(logFile, "Task CreateHostsRecord is finished successfully");
})
.OnError(exception =>
{
    LogMessage(logFile, exception.Message);
    throw exception;
});

Task("Start")
    .IsDependentOn("CreateDatabase")
    .IsDependentOn("CreateAPIWebsite")
    .IsDependentOn("CreateWebAppWebsite")
    .IsDependentOn("CreateHostsRecord")
    .IsDependentOn("BuildAPI")
    .IsDependentOn("BuildWebApp")
    .IsDependentOn("ExecuteMigrations")
    .Does(() => 
{
    System.Diagnostics.Process.Start("http://" + webAppHostName);
    LogMessage(logFile, "Task Start is finished successfully");
})
.OnError(exception =>
{
    LogMessage(logFile, exception.Message);
    throw exception;
});

Task("OnlyDbAndDependencies")
    .IsDependentOn("CreateDatabase")
    .IsDependentOn("AddOrganization")
    .IsDependentOn("Restore")
    .IsDependentOn("BuildWebApp")
    .Does(() =>
{
    LogMessage(logFile, "Task OnlyDbAndDependencies is finished successfully");
})
.OnError(exception => 
{
    LogMessage(logFile, exception.Message);
    throw exception;
});

private string NormalizeConnectionString(string connectionString)
{
    string normalizedConnectionString = connectionString;
    if(connectionString.Substring(connectionString.Length - 1) != ";")
    {
        normalizedConnectionString = normalizedConnectionString + ";";
    }

    return normalizedConnectionString;
}

private void LogMessage(FilePath filePath, string message)
{
    var formattedMessage = string.Format("[{0}] {1} \r\n", DateTime.Now.ToString("HH:mm:ss"), message);
    FileAppendText(filePath, formattedMessage);
}

private void DropDatabaseIfNeeded(string drop, string databaseName)
{
    if(drop == "true" && DatabaseExists(connectionString, databaseName))
    {
        DropDatabase(connectionString, databaseName);
        LogMessage(logFile, "Dropped database: " + databaseName);
    }
}

private void AlterJobsDb(SqlConnection connection)
{
    var dbAlterScript = string.Format(@"ALTER DATABASE [{0}] SET COMPATIBILITY_LEVEL = 130
    GO

    IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
    BEGIN
        EXEC [{0}].[dbo].[sp_fulltext_database] @action = 'enable'
    END
    GO

    ALTER DATABASE [{0}] SET ANSI_NULL_DEFAULT OFF 
    GO
    ALTER DATABASE [{0}] SET ANSI_NULLS OFF 
    GO
    ALTER DATABASE [{0}] SET ANSI_PADDING OFF 
    GO
    ALTER DATABASE [{0}] SET ANSI_WARNINGS OFF 
    GO
    ALTER DATABASE [{0}] SET ARITHABORT OFF 
    GO
    ALTER DATABASE [{0}] SET AUTO_CLOSE OFF 
    GO
    ALTER DATABASE [{0}] SET AUTO_SHRINK OFF 
    GO
    ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS ON 
    GO
    ALTER DATABASE [{0}] SET CURSOR_CLOSE_ON_COMMIT OFF 
    GO
    ALTER DATABASE [{0}] SET CURSOR_DEFAULT  GLOBAL 
    GO
    ALTER DATABASE [{0}] SET CONCAT_NULL_YIELDS_NULL OFF 
    GO
    ALTER DATABASE [{0}] SET NUMERIC_ROUNDABORT OFF 
    GO
    ALTER DATABASE [{0}] SET QUOTED_IDENTIFIER OFF 
    GO
    ALTER DATABASE [{0}] SET RECURSIVE_TRIGGERS OFF 
    GO
    ALTER DATABASE [{0}] SET  DISABLE_BROKER 
    GO
    ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
    GO
    ALTER DATABASE [{0}] SET DATE_CORRELATION_OPTIMIZATION OFF 
    GO
    ALTER DATABASE [{0}] SET TRUSTWORTHY OFF 
    GO
    ALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLATION OFF 
    GO
    ALTER DATABASE [{0}] SET PARAMETERIZATION SIMPLE 
    GO
    ALTER DATABASE [{0}] SET READ_COMMITTED_SNAPSHOT OFF 
    GO
    ALTER DATABASE [{0}] SET HONOR_BROKER_PRIORITY OFF 
    GO
    ALTER DATABASE [{0}] SET RECOVERY SIMPLE 
    GO
    ALTER DATABASE [{0}] SET  MULTI_USER 
    GO
    ALTER DATABASE [{0}] SET PAGE_VERIFY CHECKSUM  
    GO
    ALTER DATABASE [{0}] SET DB_CHAINING OFF 
    GO
    ALTER DATABASE [{0}] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
    GO
    ALTER DATABASE [{0}] SET TARGET_RECOVERY_TIME = 60 SECONDS 
    GO
    ALTER DATABASE [{0}] SET DELAYED_DURABILITY = DISABLED 
    GO
    ALTER DATABASE [{0}] SET QUERY_STORE = OFF
    GO
    ALTER DATABASE [{0}] SET  READ_WRITE
    GO", jobsDbName);

    ExecuteSqlCommand(connectionString, dbAlterScript);
}

private FilePath GetMSBuildPath(IFileSystem fileSystem, ICakeEnvironment environment, MSBuildPlatform buildPlatform)
{
    var visualStudioPath = VSWhereLatest();

    if (visualStudioPath == null)
    {
        return null;
    }

    var binPath = visualStudioPath.Combine("MSBuild/Current/Bin");
    Information("MSBuild path: {0}", binPath);

    if (fileSystem.Exist(binPath))
    {
        if (buildPlatform == MSBuildPlatform.Automatic)
        {
            if (environment.Platform.Is64Bit)
            {
                binPath = binPath.Combine("amd64");
            }
        }
        if (buildPlatform == MSBuildPlatform.x64)
        {
            binPath = binPath.Combine("amd64");
        }
    }
    else
    {
        binPath = visualStudioPath.Combine("Microsoft Visual Studio/2019/Professional/MSBuild/16.0/Bin");
    }

    return binPath.CombineWithFilePath("MSBuild.exe");
}

RunTarget(target);