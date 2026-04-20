using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <summary>
    /// Creates the 5 standard ASP.NET Core Identity tables missing from brownfield databases.
    /// FK column widths are derived at runtime from the actual AspNetRoles/AspNetUsers Id
    /// column length so the migration works on both old Identity v2 databases (NVARCHAR(128))
    /// and fresh EF Core installs (NVARCHAR(450)).
    /// </summary>
    public partial class AddMissingIdentityTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetRoleClaims')
BEGIN
    DECLARE @idLen NVARCHAR(10);
    SELECT @idLen = CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AspNetRoles' AND COLUMN_NAME = 'Id';
    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'CREATE TABLE dbo.AspNetRoleClaims (
        Id         INT           IDENTITY(1,1) NOT NULL,
        RoleId     NVARCHAR(' + @idLen + N') NOT NULL,
        ClaimType  NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId
            FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles (Id) ON DELETE CASCADE
    ); CREATE INDEX IX_AspNetRoleClaims_RoleId ON dbo.AspNetRoleClaims (RoleId);';
    EXEC(@sql);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUserClaims')
BEGIN
    DECLARE @idLen NVARCHAR(10);
    SELECT @idLen = CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Id';
    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'CREATE TABLE dbo.AspNetUserClaims (
        Id         INT           IDENTITY(1,1) NOT NULL,
        UserId     NVARCHAR(' + @idLen + N') NOT NULL,
        ClaimType  NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetUserClaims PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
    ); CREATE INDEX IX_AspNetUserClaims_UserId ON dbo.AspNetUserClaims (UserId);';
    EXEC(@sql);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUserLogins')
BEGIN
    DECLARE @idLen NVARCHAR(10);
    SELECT @idLen = CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Id';
    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'CREATE TABLE dbo.AspNetUserLogins (
        LoginProvider       NVARCHAR(' + @idLen + N') NOT NULL,
        ProviderKey         NVARCHAR(' + @idLen + N') NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId              NVARCHAR(' + @idLen + N') NOT NULL,
        CONSTRAINT PK_AspNetUserLogins PRIMARY KEY CLUSTERED (LoginProvider, ProviderKey),
        CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
    ); CREATE INDEX IX_AspNetUserLogins_UserId ON dbo.AspNetUserLogins (UserId);';
    EXEC(@sql);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUserRoles')
BEGIN
    DECLARE @userIdLen NVARCHAR(10);
    DECLARE @roleIdLen NVARCHAR(10);
    SELECT @userIdLen = CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Id';
    SELECT @roleIdLen = CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AspNetRoles' AND COLUMN_NAME = 'Id';
    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'CREATE TABLE dbo.AspNetUserRoles (
        UserId NVARCHAR(' + @userIdLen + N') NOT NULL,
        RoleId NVARCHAR(' + @roleIdLen + N') NOT NULL,
        CONSTRAINT PK_AspNetUserRoles PRIMARY KEY CLUSTERED (UserId, RoleId),
        CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE,
        CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId
            FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles (Id) ON DELETE CASCADE
    ); CREATE INDEX IX_AspNetUserRoles_RoleId ON dbo.AspNetUserRoles (RoleId);';
    EXEC(@sql);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUserTokens')
BEGIN
    DECLARE @idLen NVARCHAR(10);
    SELECT @idLen = CAST(CHARACTER_MAXIMUM_LENGTH AS NVARCHAR(10)) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Id';
    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'CREATE TABLE dbo.AspNetUserTokens (
        UserId        NVARCHAR(' + @idLen + N') NOT NULL,
        LoginProvider NVARCHAR(128) NOT NULL,
        Name          NVARCHAR(128) NOT NULL,
        Value         NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetUserTokens PRIMARY KEY CLUSTERED (UserId, LoginProvider, Name),
        CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
    );';
    EXEC(@sql);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.AspNetUserTokens");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.AspNetUserRoles");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.AspNetUserLogins");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.AspNetUserClaims");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.AspNetRoleClaims");
        }
    }
}
