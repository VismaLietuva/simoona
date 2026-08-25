using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddVideoLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTypes_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoLibraryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PictureId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VideoTypeId = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoLibraryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoLibraryItems_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoLibraryItems_VideoTypes_VideoTypeId",
                        column: x => x.VideoTypeId,
                        principalTable: "VideoTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibraryItems_OrganizationId_Created",
                table: "VideoLibraryItems",
                columns: new[] { "OrganizationId", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibraryItems_OrganizationId_VideoTypeId",
                table: "VideoLibraryItems",
                columns: new[] { "OrganizationId", "VideoTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibraryItems_VideoTypeId",
                table: "VideoLibraryItems",
                column: "VideoTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTypes_OrganizationId_Title",
                table: "VideoTypes",
                columns: new[] { "OrganizationId", "Title" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'VIDEOLIBRARY_BASIC')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'VIDEOLIBRARY_BASIC', GETDATE(), NULL, GETDATE(), NULL, 0, N'basic', NULL)
END

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'VIDEOLIBRARY_ADMINISTRATION')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'VIDEOLIBRARY_ADMINISTRATION', GETDATE(), NULL, GETDATE(), NULL, 0, N'admin', NULL)
END

DECLARE @basicId  INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'VIDEOLIBRARY_BASIC')
DECLARE @adminId  INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'VIDEOLIBRARY_ADMINISTRATION')
DECLARE @mirrorBasicId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'EMPLOYEELIST_BASIC')
DECLARE @mirrorAdminId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'ORGANIZATION_ADMINISTRATION')

IF @basicId IS NOT NULL
BEGIN
    INSERT dbo.RolePermissions ([PermissionId], [RoleId])
    SELECT DISTINCT @basicId, rp.RoleId
    FROM   dbo.RolePermissions rp
    WHERE  rp.PermissionId IN (@mirrorBasicId, @mirrorAdminId)
      AND  NOT EXISTS (
               SELECT 1 FROM dbo.RolePermissions rp2
               WHERE  rp2.PermissionId = @basicId AND rp2.RoleId = rp.RoleId
           )
END

IF @adminId IS NOT NULL AND @mirrorAdminId IS NOT NULL
BEGIN
    INSERT dbo.RolePermissions ([PermissionId], [RoleId])
    SELECT @adminId, rp.RoleId
    FROM   dbo.RolePermissions rp
    WHERE  rp.PermissionId = @mirrorAdminId
      AND  NOT EXISTS (
               SELECT 1 FROM dbo.RolePermissions rp2
               WHERE  rp2.PermissionId = @adminId AND rp2.RoleId = rp.RoleId
           )
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @ids TABLE (Id INT)
INSERT @ids SELECT Id FROM dbo.Permissions WHERE Name IN (N'VIDEOLIBRARY_BASIC', N'VIDEOLIBRARY_ADMINISTRATION')

DELETE FROM dbo.RolePermissions WHERE PermissionId IN (SELECT Id FROM @ids)
DELETE FROM dbo.Permissions     WHERE Id IN (SELECT Id FROM @ids)
");

            migrationBuilder.DropTable(
                name: "VideoLibraryItems");

            migrationBuilder.DropTable(
                name: "VideoTypes");
        }
    }
}
