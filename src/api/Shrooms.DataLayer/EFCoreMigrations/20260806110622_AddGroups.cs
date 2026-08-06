using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsTemporary = table.Column<bool>(type: "bit", nullable: false),
                    CreationPolicy = table.Column<int>(type: "int", nullable: false),
                    ApprovalQuestions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasGroupTag = table.Column<bool>(type: "bit", nullable: false),
                    KudosTypeId = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupTypes_KudosTypes_KudosTypeId",
                        column: x => x.KudosTypeId,
                        principalTable: "KudosTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupTypes_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    PictureId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupTypeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovalAnswers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_GroupTypes_GroupTypeId",
                        column: x => x.GroupTypeId,
                        principalTable: "GroupTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groups_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPubliclyVisible = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupReferences_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_GroupId_UserId",
                table: "GroupMembers",
                columns: new[] { "GroupId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupReferences_GroupId",
                table: "GroupReferences",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_GroupTypeId",
                table: "Groups",
                column: "GroupTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_OrganizationId_Name",
                table: "Groups",
                columns: new[] { "OrganizationId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupTypes_KudosTypeId",
                table: "GroupTypes",
                column: "KudosTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTypes_OrganizationId",
                table: "GroupTypes",
                column: "OrganizationId");

            // Seed the five predefined group types for every organization.
            // CreationPolicy: 0 = administrators only, 1 = anyone, 2 = anyone with approval.
            // Committee and FoodMaster point at their matching kudos types.
            // All settings stay editable afterwards - these are defaults, not system types.
            migrationBuilder.Sql(@"
DECLARE @committeeKudos INT = (SELECT TOP 1 Id FROM dbo.KudosTypes WHERE Name = N'Committee Membership' AND IsDeleted = 0)
DECLARE @foodKudos      INT = (SELECT TOP 1 Id FROM dbo.KudosTypes WHERE Name LIKE N'Food master%' AND IsDeleted = 0)

DECLARE @taskForceQuestions NVARCHAR(MAX) = N'**List the members of the taskforce:**

**List the goals of the taskforce:**

**How many members do you expect in this taskforce?**

**How are you going to select members?**

**When the taskforce should be concluded?**

**How many hours in a month would 1 member of the taskforce need?**

**Name required budget**

**Additional notes**
'

INSERT INTO dbo.GroupTypes
    ([Name], [OrganizationId], [SortOrder], [IsTemporary], [HasGroupTag], [KudosTypeId],
     [CreationPolicy], [ApprovalQuestions],
     [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted])
SELECT t.[Name], o.[Id], t.[SortOrder], t.[IsTemporary], t.[HasGroupTag], t.[KudosTypeId],
       t.[CreationPolicy], t.[ApprovalQuestions],
       GETDATE(), NULL, GETDATE(), NULL, 0
FROM   dbo.Organizations o
CROSS JOIN (VALUES
    (N'Committee',       1, 0, 1, @committeeKudos, 0, NULL),
    (N'TaskForce',       2, 1, 1, NULL,            2, @taskForceQuestions),
    (N'FoodMaster',      3, 0, 1, @foodKudos,      0, NULL),
    (N'GroupOfInterest', 4, 0, 1, NULL,            1, NULL),
    (N'Other',           5, 0, 0, NULL,            1, NULL)
) AS t([Name], [SortOrder], [IsTemporary], [HasGroupTag], [KudosTypeId], [CreationPolicy], [ApprovalQuestions])
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.GroupTypes gt
    WHERE gt.[Name] = t.[Name] AND gt.[OrganizationId] = o.[Id]
)
");

            // Seed GROUPS permissions, mirroring the roles that hold the committee equivalents.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'GROUPS_BASIC')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'GROUPS_BASIC', GETDATE(), NULL, GETDATE(), NULL, 0, N'basic', NULL)
END

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'GROUPS_ADMINISTRATION')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'GROUPS_ADMINISTRATION', GETDATE(), NULL, GETDATE(), NULL, 0, N'admin', NULL)
END

DECLARE @groupsBasic     INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'GROUPS_BASIC')
DECLARE @groupsAdmin     INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'GROUPS_ADMINISTRATION')
DECLARE @committeeBasic  INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'COMMITTEES_BASIC')
DECLARE @committeeAdmin  INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'COMMITTEES_ADMINISTRATION')

IF @groupsBasic IS NOT NULL AND @committeeBasic IS NOT NULL
BEGIN
    INSERT dbo.RolePermissions ([PermissionId], [RoleId])
    SELECT @groupsBasic, rp.RoleId
    FROM   dbo.RolePermissions rp
    WHERE  rp.PermissionId = @committeeBasic
      AND  NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp2
                       WHERE rp2.PermissionId = @groupsBasic AND rp2.RoleId = rp.RoleId)
END

IF @groupsAdmin IS NOT NULL AND @committeeAdmin IS NOT NULL
BEGIN
    INSERT dbo.RolePermissions ([PermissionId], [RoleId])
    SELECT @groupsAdmin, rp.RoleId
    FROM   dbo.RolePermissions rp
    WHERE  rp.PermissionId = @committeeAdmin
      AND  NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp2
                       WHERE rp2.PermissionId = @groupsAdmin AND rp2.RoleId = rp.RoleId)
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The seeded group types need no explicit cleanup - dropping GroupTypes removes them.
            migrationBuilder.Sql(@"
DECLARE @ids TABLE (Id INT)
INSERT INTO @ids SELECT Id FROM dbo.Permissions WHERE Name IN (N'GROUPS_BASIC', N'GROUPS_ADMINISTRATION')

DELETE FROM dbo.RolePermissions WHERE PermissionId IN (SELECT Id FROM @ids)
DELETE FROM dbo.Permissions     WHERE Id IN (SELECT Id FROM @ids)
");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "GroupReferences");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "GroupTypes");
        }
    }
}
