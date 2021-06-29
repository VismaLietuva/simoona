# Simoona API

Simoona's back-end is built using ASP.NET with Entity Framework. Project dependencies are managed via NuGet.

## QuickStart

1. Open the solution file `Shrooms.sln` file located in `src\api\Shrooms.sln`.
2. Set `Shrooms.Presentation.Api` project as StartUp project (located in `src\api\Main\PresentationLayer\Shrooms.Presentation.Api\`).
3. Start it with debug (F5) or without debug (Ctrl+F5).
4. Wait for project to build.
5. If database is set up and Web.config has correct connection string the project should start. To set up the database head over to [build](../../build) folder.

## SMTP Client Setup

Simoona uses email sending services to send its users various important and optional notifications. If you don't set up the mailing service, users won't be able to receive these notifications. It's especially important to set it up if local sign-up/sign-in system is being used (by default it is), because the system uses email service to send information about verifying email addresses and resetting passwords. For development purposes the easiest way to set up SMTP Server is to use [MailTrap](https://mailtrap.io/). For sending emails to their destinations we recommend services like [SendGrid](https://sendgrid.com/).

Follow these steps to configure mail sending service in Simoona:

1. Create a service account using [MailTrap](https://mailtrap.io/), [SendGrid](https://sendgrid.com/) or any other email service
2. After creating an account find your SMTP server credentials on their website
3. Open Web.config file located in `src\api\Main\PresentationLayer\Shrooms.Presentation.Api\Web.config`
4. Locate `<mailSettings>` block in Web.config file and paste your credentials and host to appropriate places inside of `<network>` block as shown in code snippet bellow

    ```xml
    <system.net>
      <mailSettings>
        <smtp from="noreply@simoona.com">
            <network host="yourHost" userName="yourUserName" password="yourPassword" />
        </smtp>
      </mailSettings>
    </system.net>
    ```

Another way to work with email services while developing is installing [smtp4dev](https://github.com/smorks/smtp4dev/releases/latest) and setting your `Web.config` according to this:

    ```xml
    <system.net>
      <mailSettings>
        <smtp from="noreply@simoona.com">
            <network host="localhost" port="25" />
        </smtp>
      </mailSettings>
    </system.net>
    ```
	
You will see all your sent emails in the dedicated application on your local machine while actually no third party or online service is involved.

## Configuration

All configurable properties are located in `src\api\Main\PresentationLayer\Shrooms.Presentation.Api\Web.config` file.

Don't forget to change API and Client urls if running on different addresses:

```xml
<!-- Url used to redirect back to AngularJS application after external login -->
<add key="OAuthRedirectUri" value="http://localhost:3000/" />

<!-- Urls for AngularJS application and API -->
<add key="ClientUrl" value="http://localhost:3000/" />
<add key="ApiUrl" value="http://localhost:50321" />
```

## Optional Features

### 1. Azure Blob Storage

By default Simoona uses local file system storage, but it's possible to use Azure Blob Storage for storing media files. To set up blob storage follow these steps:

 1. Head over to [Azure Portal](https://portal.azure.com/) and follow instructions [Azure Quickstart](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-portal)
 2. After creating blob storage get its connection string
 3. Open `Web.config` file located in `src\api\Main\PresentationLayer\Shrooms.Presentation.Api\`
 4. Locate `<connectionStrings>` block and put your blob storage connection string as a value of `connectionString` inside `<add>` block with `StorageConnectionString` name as shown below

    ```xml
    <connectionStrings>
        ...
        <add name="StorageConnectionString" connectionString="yourConnectionString" />
    </connectionStrings>
    ```

If you don't use [ImageResizer](http://imageresizing.net/) Performance Edition plugins, but wish to use Azure Blob Storage you will need to follow these extra steps:

1. Open Web.config file located in `src\api\Main\PresentationLayer\Shrooms.Presentation.Api\Web.config`
2. Inside `<rules>` block in `<system.webServer>` block find `Redirect to Azure Blob Storage` rule, make sure that it is uncommented and paste your blob storage url as `url` value inside `<action>` block as shown bellow (make sure to leave `{R:1}` intact)

    ```xml
    <system.webServer>
      <rewrite>
        <rules>
          <rule name="Redirect to Azure Blob Storage">
            <match url="^storage/(.*)$"/>
            <action type="Redirect" url="https://your-project.blob.core.windows.net/{R:1}" redirectType="Permanent" />
          </rule>
        </rules>
      </rewrite>
    </system.webServer>
    ```

### 2. Google and Facebook Sign-in

Simoona supports sign in with Google and Facebook. To enable this feature follow these steps:

1. Get [Google](https://console.developers.google.com/projectselector/apis/credentials) and/or [Facebook](https://developers.facebook.com/docs/apps/register/#app-settings) application credentials
2. Open Web.config file located in `src\api\Main\PresentationLayer\Shrooms.Presentation.Api\`
3. Locate `<appSettings>` block and find the lines shown below and just paste your application credentials to appropriate places as `value`

  ```xml
  <appSettings>
    ...
    <add key="GoogleAccountClientId" value="yourGoogleAccountClientId" />
    <add key="GoogleAccountClientSecret" value="yourGoogleAccountClientSecret" />
    <add key="FacebookAccountAppId" value="yourFacebookAccountAppId" />
    <add key="FacebookAccountAppSecret" value="yourFacebookAccountAppSecret" />
    ...
  </appSettings>
  ```

### 3. ImageResizer Plugins

Simoona can also leverage [ImageResizer Performance plugins](http://imageresizing.net/plugins/editions/performance) to make media delivery faster. If you wish to try these plugins in development environment or you have a license to use them follow these steps:

1. Open Web.config file located in `src\api\Main\PresentationLayer\Shrooms.Presentation.Api\`
2. Locate `<resizer>` block and uncomment lines inside of `<plugins>` block as shown bellow

    ```xml
    <resizer>
      <plugins>
        <add name="AzureReader2" prefix="~/storage" connectionString="yourBlobStorageConnectionString" endpoint="https://your-project.blob.core.windows.net/" />
        <add name="AnimatedGifs" />
        <add name="DiskCache" />
        <add name="PrettyGifs" />
      </plugins>
    </resizer>
    ```

To use [AzureReader2](http://imageresizing.net/docs/v4/plugins/azurereader2) plugin you will have to provder your `connectionString` and `endpoint`, paste these values to appropriate places inside `add` block with `name="AzureReader2"` attribute. If you have a `Redirect to Azure Blob Storage` rule enabled don't forget to disable it.

To use these plugins in production environment you will have to put [ImageResizer license](https://imageresizing.net/pricing) inside `<license>` block as shown below

```xml
<resizer>
  <licenses>
    <license>
      <!-- ImageResizer license here -->
    </license>
  </licenses>
</resizer>
```

## EntityFramework Database Migrations

1. Open model (e.g. `ServiceRequest.cs`) and add new property, for example:

```csharp
public string PictureId { get; set; }
```

2. Open Package Manager Console in Visual Studio and call command:

```
add-migration MigrationName -ConnectionString "ConnectionString" -ConnectionProviderName "System.Data.SqlClient" -StartUpProjectName Shrooms.Presentation.Api -ProjectName Shrooms.DataLayer
```

Connection string can be found in `Web.config`.

3. Once it was done - new migration file should appear in `DataLayer` project. Content should be something like:

```csharp
AddColumn("dbo.ServiceRequests", "PictureId", c => c.String());
```

4. To apply migration on the database, execute following command in Package Manager Console:

```
update-database -verbose -ConnectionString "ConnectionString" -ConnectionProviderName "System.Data.SqlClient" -StartUpProjectName Shrooms.Presentation.Api -ProjectName Shrooms.DataLayer
```

4.1 To rollback to previous migration, execute following command:

```
update-database  -verbose -ConnectionString "ConnectionString" -ConnectionProviderName "System.Data.SqlClient" -StartUpProjectName Shrooms.Presentation.Api -ProjectName Shrooms.DataLayer –TargetMigration "202001021211276_MigrationName"
```

5. Migration is done

For more details, please reffer to
https://msdn.microsoft.com/en-us/library/jj591621(v=vs.113).aspx or 
http://www.entityframeworktutorial.net/code-first/code-based-migration-in-code-first.aspx

