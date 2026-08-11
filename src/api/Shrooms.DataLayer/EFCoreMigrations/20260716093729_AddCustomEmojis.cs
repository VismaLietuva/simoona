using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddCustomEmojis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomEmojis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BlobName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomEmojis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomEmojis_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomEmojis_OrganizationId_Name",
                table: "CustomEmojis",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'CUSTOMEMOJI_BASIC')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'CUSTOMEMOJI_BASIC', GETDATE(), NULL, GETDATE(), NULL, 0, N'basic', NULL)
END

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'CUSTOMEMOJI_ADMINISTRATION')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'CUSTOMEMOJI_ADMINISTRATION', GETDATE(), NULL, GETDATE(), NULL, 0, N'admin', NULL)
END

DECLARE @basicId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'CUSTOMEMOJI_BASIC')
DECLARE @pictureBasicId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'PICTURE_BASIC')

IF @basicId IS NOT NULL AND @pictureBasicId IS NOT NULL
BEGIN
    INSERT dbo.RolePermissions ([PermissionId], [RoleId])
    SELECT @basicId, rp.RoleId
    FROM dbo.RolePermissions rp
    WHERE rp.PermissionId = @pictureBasicId
      AND NOT EXISTS (
            SELECT 1 FROM dbo.RolePermissions rp2
            WHERE rp2.PermissionId = @basicId AND rp2.RoleId = rp.RoleId
          )
END

DECLARE @adminId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'CUSTOMEMOJI_ADMINISTRATION')
DECLARE @wallAdminId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'WALL_ADMINISTRATION')

IF @adminId IS NOT NULL AND @wallAdminId IS NOT NULL
BEGIN
    INSERT dbo.RolePermissions ([PermissionId], [RoleId])
    SELECT @adminId, rp.RoleId
    FROM dbo.RolePermissions rp
    WHERE rp.PermissionId = @wallAdminId
      AND NOT EXISTS (
            SELECT 1 FROM dbo.RolePermissions rp2
            WHERE rp2.PermissionId = @adminId AND rp2.RoleId = rp.RoleId
          )
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @basicId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'CUSTOMEMOJI_BASIC')
IF @basicId IS NOT NULL
BEGIN
    DELETE FROM dbo.RolePermissions WHERE PermissionId = @basicId
    DELETE FROM dbo.Permissions WHERE Id = @basicId
END

DECLARE @adminId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'CUSTOMEMOJI_ADMINISTRATION')
IF @adminId IS NOT NULL
BEGIN
    DELETE FROM dbo.RolePermissions WHERE PermissionId = @adminId
    DELETE FROM dbo.Permissions WHERE Id = @adminId
END
");

            migrationBuilder.DropTable(
                name: "CustomEmojis");
        }
    }
}
