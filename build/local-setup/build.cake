#addin "nuget:?package=Cake.SqlServer"
#addin "Cake.IIS"
#addin "Microsoft.Win32.Registry"
#addin "System.Reflection.TypeExtensions"
#addin "Cake.Hosts"
#addin "Cake.FileHelpers"

using System.Data.SqlClient;

var target = Argument("target", "Default");
var organization = Argument("organization", "testorg");
var email = Argument("email", "tester@example.com");
var dbName = Argument("dbName", "SimoonaDb");
var connectionString = Argument("connectionString", "Data Source=localhost\\SQLEXPRESS;Integrated Security=True;Connect Timeout=60; MultipleActiveResultSets=True;");
var jobsDbName = dbName + "Jobs";
var dropDb = Argument("dropdb", "true");
var APIpath = "api/";
var webAppPath = "webapp/";
var webAppHostName = "app.simoona.local";
var applicationPool = "Simoona";
FilePath logFile;

Setup(context =>
{
    CreateDirectory("./logs");
    logFile = new FilePath(String.Format("logs/{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmss")));

    Information("ConnectionString: {0}", connectionString);
});

TaskSetup(setupContext =>
{
    var message = string.Format("Task {0} is starting", setupContext.Task.Name);
    LogMessage(logFile, message);
});

Task("Default")
    .IsDependentOn("CreateDatabase")
    .IsDependentOn("AddOrganization")
    .IsDependentOn("CreateAPIWebsite")
    .IsDependentOn("CreateWebAppWebsite")
    .IsDependentOn("CreateHostsRecord")
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

Task("AddOrganization")
    .IsDependentOn("CreateDatabase")
    .Does(() => 
{
    string xmlFile = APIpath + "Web.config";
    System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
    xmlDoc.Load(xmlFile);

    var organizationsNode = xmlDoc.SelectSingleNode("configuration/RegisteredOrganizations");
    organizationsNode.RemoveAll();

    var organizationElement = xmlDoc.CreateElement("add");
    organizationElement.SetAttribute("key", organization);
    organizationElement.SetAttribute("value", organization);
    organizationsNode.AppendChild(organizationElement);

    string conn = connectionString;
    if(connectionString.Substring(connectionString.Length - 1) != ";")
    {
        conn = conn + ";";
    }

    var mainConnectionString = xmlDoc.SelectSingleNode("configuration/connectionStrings/add[not(@name = 'BackgroundJobs') and not(@name = 'StorageConnectionString')]");

    mainConnectionString.Attributes["name"].Value = organization;
    mainConnectionString.Attributes["connectionString"].Value = string.Format("{0}Database={1};", conn, dbName);

    var jobsConnectionString = xmlDoc.SelectSingleNode("configuration/connectionStrings/add[@name = 'BackgroundJobs']");
    jobsConnectionString.Attributes["connectionString"].Value = string.Format("{0}Database={1};", conn, jobsDbName);

    xmlDoc.Save(xmlFile);
    using (var connection = OpenSqlConnection(connectionString))
    {
        ExecuteSqlCommand(connection, "USE \""+dbName+"\"");
        ExecuteSqlCommand(connection, "UPDATE dbo.Organizations SET Name='"+organization+"', ShortName='"+organization+"', AuthenticationProviders='internal;google;facebook' WHERE Id=1");
    }
    LogMessage(logFile, "Task AddOrganization is finished successfully");
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
        PhysicalDirectory = APIpath,
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
        PhysicalDirectory = webAppPath,
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

RunTarget(target);
