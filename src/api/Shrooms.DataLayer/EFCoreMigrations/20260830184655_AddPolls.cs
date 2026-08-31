using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    public partial class AddPolls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Polls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    IsOfficial = table.Column<bool>(type: "bit", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    State = table.Column<int>(type: "int", nullable: false),
                    WallId = table.Column<int>(type: "int", nullable: false),
                    ReviewedById = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Polls_AspNetUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Polls_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Polls_Walls_WallId",
                        column: x => x.WallId,
                        principalTable: "Walls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PollParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PollId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollParticipants_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PollParticipants_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PollQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PollId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AllowMultiple = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollQuestions_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PollOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PollQuestionId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollOptions_PollQuestions_PollQuestionId",
                        column: x => x.PollQuestionId,
                        principalTable: "PollQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PollAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PollId = table.Column<int>(type: "int", nullable: false),
                    PollQuestionId = table.Column<int>(type: "int", nullable: false),
                    PollOptionId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollAnswers_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PollAnswers_PollOptions_PollOptionId",
                        column: x => x.PollOptionId,
                        principalTable: "PollOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PollAnswers_PollQuestions_PollQuestionId",
                        column: x => x.PollQuestionId,
                        principalTable: "PollQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PollAnswers_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswers_ApplicationUserId",
                table: "PollAnswers",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswers_PollId",
                table: "PollAnswers",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswers_PollOptionId",
                table: "PollAnswers",
                column: "PollOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswers_PollQuestionId",
                table: "PollAnswers",
                column: "PollQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PollOptions_PollQuestionId",
                table: "PollOptions",
                column: "PollQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PollParticipants_ApplicationUserId",
                table: "PollParticipants",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PollParticipants_PollId_ApplicationUserId",
                table: "PollParticipants",
                columns: new[] { "PollId", "ApplicationUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PollQuestions_PollId",
                table: "PollQuestions",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_Polls_OrganizationId_State",
                table: "Polls",
                columns: new[] { "OrganizationId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Polls_ReviewedById",
                table: "Polls",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_Polls_WallId",
                table: "Polls",
                column: "WallId");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'POLL_BASIC')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'POLL_BASIC', GETDATE(), NULL, GETDATE(), NULL, 0, N'basic', NULL)
END

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Name = N'POLL_ADMINISTRATION')
BEGIN
    INSERT dbo.Permissions ([Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId])
    VALUES (N'POLL_ADMINISTRATION', GETDATE(), NULL, GETDATE(), NULL, 0, N'administration', NULL)
END

DECLARE @pollBasic INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'POLL_BASIC')
DECLARE @pollAdmin INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'POLL_ADMINISTRATION')
DECLARE @eventBasic INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'EVENT_BASIC')
DECLARE @eventAdmin INT = (SELECT Id FROM dbo.Permissions WHERE Name = N'EVENT_ADMINISTRATION')

IF @pollBasic IS NOT NULL AND @eventBasic IS NOT NULL
BEGIN
    INSERT dbo.RolePermissions ([PermissionId], [RoleId])
    SELECT @pollBasic, rp.RoleId
    FROM   dbo.RolePermissions rp
    WHERE  rp.PermissionId = @eventBasic
      AND  NOT EXISTS (SELECT 1 FROM dbo.RolePermissions x WHERE x.PermissionId = @pollBasic AND x.RoleId = rp.RoleId)
END

IF @pollAdmin IS NOT NULL AND @eventAdmin IS NOT NULL
BEGIN
    INSERT dbo.RolePermissions ([PermissionId], [RoleId])
    SELECT @pollAdmin, rp.RoleId
    FROM   dbo.RolePermissions rp
    WHERE  rp.PermissionId = @eventAdmin
      AND  NOT EXISTS (SELECT 1 FROM dbo.RolePermissions x WHERE x.PermissionId = @pollAdmin AND x.RoleId = rp.RoleId)
END
");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @ids TABLE (Id INT)
INSERT @ids SELECT Id FROM dbo.Permissions WHERE Name IN (N'POLL_BASIC', N'POLL_ADMINISTRATION')
DELETE FROM dbo.RolePermissions WHERE PermissionId IN (SELECT Id FROM @ids)
DELETE FROM dbo.Permissions WHERE Id IN (SELECT Id FROM @ids)
");

            migrationBuilder.DropTable(
                name: "PollAnswers");

            migrationBuilder.DropTable(
                name: "PollParticipants");

            migrationBuilder.DropTable(
                name: "PollOptions");

            migrationBuilder.DropTable(
                name: "PollQuestions");

            migrationBuilder.DropTable(
                name: "Polls");
        }
    }
}
