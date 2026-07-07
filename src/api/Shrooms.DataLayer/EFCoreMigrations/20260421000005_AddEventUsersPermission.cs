using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    public partial class AddEventUsersPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EVENTUSERS_BASIC was present in brownfield databases seeded by the old EF6 app
            // but was omitted from the EF Core SeedInitialData migration.
            // Mirror its RolePermissions from EVENT_BASIC so every role that can use events
            // can also see event participants.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'EVENTUSERS_BASIC')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'EVENTUSERS_BASIC', GETDATE(), NULL, GETDATE(), NULL, 0, N'basic', NULL)
END

DECLARE @permId      INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'EVENTUSERS_BASIC')
DECLARE @eventBasicId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'EVENT_BASIC')

IF @permId IS NOT NULL AND @eventBasicId IS NOT NULL
BEGIN
    INSERT dbo.RolePermissions ([PermissionId], [RoleId])
    SELECT @permId, rp.RoleId
    FROM   dbo.RolePermissions rp
    WHERE  rp.PermissionId = @eventBasicId
      AND  NOT EXISTS (
               SELECT 1 FROM dbo.RolePermissions rp2
               WHERE  rp2.PermissionId = @permId AND rp2.RoleId = rp.RoleId
           )
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @permId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'EVENTUSERS_BASIC')
IF @permId IS NOT NULL
BEGIN
    DELETE FROM dbo.RolePermissions WHERE PermissionId = @permId
    DELETE FROM dbo.Permissions       WHERE Id = @permId
END
");
        }
    }
}
