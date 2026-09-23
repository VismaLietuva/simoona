using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddGroupKudosAwarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GroupKudosPeriod",
                table: "KudosLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AwardTemplate",
                table: "GroupTypes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KudosLogs_OrganizationId_GroupKudosPeriod",
                table: "KudosLogs",
                columns: new[] { "OrganizationId", "GroupKudosPeriod" },
                filter: "[GroupKudosPeriod] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KudosLogs_OrganizationId_GroupKudosPeriod",
                table: "KudosLogs");

            migrationBuilder.DropColumn(
                name: "GroupKudosPeriod",
                table: "KudosLogs");

            migrationBuilder.DropColumn(
                name: "AwardTemplate",
                table: "GroupTypes");
        }
    }
}
