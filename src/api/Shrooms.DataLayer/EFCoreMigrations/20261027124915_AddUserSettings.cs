using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The new columns are shorter than the nvarchar(max) ones they copy from.
            // Fail loudly rather than truncate: fix the rows, then migrate again.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE LEN([CultureCode]) > 20 OR LEN([TimeZone]) > 100)
    THROW 50000, N'AddUserSettings: some AspNetUsers rows have a CultureCode over 20 or a TimeZone over 100 characters.', 1;
");

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CultureCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DailyMailingHour = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsTutorialComplete = table.Column<bool>(type: "bit", nullable: false),
                    Preferences = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSettings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // One row per user, soft-deleted ones included. The AspNetUsers columns
            // stay until a later migration drops them.
            migrationBuilder.Sql(@"
INSERT dbo.UserSettings ([UserId], [CultureCode], [TimeZone], [DailyMailingHour], [IsTutorialComplete], [Preferences], [Created], [CreatedBy], [Modified], [ModifiedBy])
SELECT [Id], [CultureCode], [TimeZone], [DailyMailingHour], [IsTutorialComplete], [WallWidgetPreferences], SYSUTCDATETIME(), NULL, SYSUTCDATETIME(), NULL
FROM dbo.AspNetUsers
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only UserSettings has been written since Up, so copy it back first.
            migrationBuilder.Sql(@"
UPDATE u
SET u.[CultureCode] = s.[CultureCode],
    u.[TimeZone] = s.[TimeZone],
    u.[DailyMailingHour] = s.[DailyMailingHour],
    u.[IsTutorialComplete] = s.[IsTutorialComplete],
    u.[WallWidgetPreferences] = s.[Preferences]
FROM dbo.AspNetUsers u
JOIN dbo.UserSettings s ON s.[UserId] = u.[Id]
");

            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
