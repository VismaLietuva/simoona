using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddTimeRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeCustomers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeCustomers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeMonthLocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Month = table.Column<DateTime>(type: "date", nullable: false),
                    LockedById = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeMonthLocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeMonthLocks_AspNetUsers_LockedById",
                        column: x => x.LockedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeMonthLocks_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimePeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Month = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimePeriods_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimePeriods_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeProjects_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeProjects_TimeCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "TimeCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeProjectMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsAutoFill = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeProjectMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeProjectMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeProjectMembers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeProjectMembers_TimeProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "TimeProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeTasks_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeTasks_TimeProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "TimeProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Minutes = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeEntries_TimeProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "TimeProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeEntries_TimeTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TimeTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeCustomers_OrganizationId_Name",
                table: "TimeCustomers",
                columns: new[] { "OrganizationId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_OrganizationId_UserId_Date",
                table: "TimeEntries",
                columns: new[] { "OrganizationId", "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_ProjectId",
                table: "TimeEntries",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_TaskId",
                table: "TimeEntries",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId_TaskId_Date",
                table: "TimeEntries",
                columns: new[] { "UserId", "TaskId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeMonthLocks_LockedById",
                table: "TimeMonthLocks",
                column: "LockedById");

            migrationBuilder.CreateIndex(
                name: "IX_TimeMonthLocks_OrganizationId_Month",
                table: "TimeMonthLocks",
                columns: new[] { "OrganizationId", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimePeriods_OrganizationId_Month",
                table: "TimePeriods",
                columns: new[] { "OrganizationId", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_TimePeriods_UserId_Month",
                table: "TimePeriods",
                columns: new[] { "UserId", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeProjectMembers_OrganizationId_UserId",
                table: "TimeProjectMembers",
                columns: new[] { "OrganizationId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeProjectMembers_ProjectId_UserId",
                table: "TimeProjectMembers",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeProjectMembers_UserId",
                table: "TimeProjectMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeProjects_CustomerId_Name",
                table: "TimeProjects",
                columns: new[] { "CustomerId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TimeProjects_OrganizationId_IsArchived",
                table: "TimeProjects",
                columns: new[] { "OrganizationId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeTasks_OrganizationId",
                table: "TimeTasks",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTasks_ProjectId_Name",
                table: "TimeTasks",
                columns: new[] { "ProjectId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'TIMEREGISTRATION_BASIC')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'TIMEREGISTRATION_BASIC', GETDATE(), NULL, GETDATE(), NULL, 0, N'basic', NULL)
END

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'TIMEREGISTRATION_ADMINISTRATION')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'TIMEREGISTRATION_ADMINISTRATION', GETDATE(), NULL, GETDATE(), NULL, 0, N'admin', NULL)
END

DECLARE @basicId  INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'TIMEREGISTRATION_BASIC')
DECLARE @adminId  INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'TIMEREGISTRATION_ADMINISTRATION')
DECLARE @mirrorBasicId INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'VACATIONS_BASIC')
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
INSERT @ids SELECT Id FROM dbo.Permissions WHERE Name IN (N'TIMEREGISTRATION_BASIC', N'TIMEREGISTRATION_ADMINISTRATION')

DELETE FROM dbo.RolePermissions WHERE PermissionId IN (SELECT Id FROM @ids)
DELETE FROM dbo.Permissions     WHERE Id IN (SELECT Id FROM @ids)
");

            migrationBuilder.DropTable(
                name: "TimeEntries");

            migrationBuilder.DropTable(
                name: "TimeMonthLocks");

            migrationBuilder.DropTable(
                name: "TimePeriods");

            migrationBuilder.DropTable(
                name: "TimeProjectMembers");

            migrationBuilder.DropTable(
                name: "TimeTasks");

            migrationBuilder.DropTable(
                name: "TimeProjects");

            migrationBuilder.DropTable(
                name: "TimeCustomers");
        }
    }
}
