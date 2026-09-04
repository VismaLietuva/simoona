using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddKudosLogsQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KudosLogs_OrganizationId",
                table: "KudosLogs");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "KudosLogs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KudosLogs_CreatedBy_Created",
                table: "KudosLogs",
                columns: new[] { "CreatedBy", "Created" })
                .Annotation("SqlServer:Include", new[] { "OrganizationId", "KudosSystemType", "Points" });

            migrationBuilder.CreateIndex(
                name: "IX_KudosLogs_OrganizationId_Status_Created",
                table: "KudosLogs",
                columns: new[] { "OrganizationId", "Status", "Created" })
                .Annotation("SqlServer:Include", new[] { "KudosSystemType", "KudosBasketId", "EmployeeId", "Points", "CreatedBy" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KudosLogs_CreatedBy_Created",
                table: "KudosLogs");

            migrationBuilder.DropIndex(
                name: "IX_KudosLogs_OrganizationId_Status_Created",
                table: "KudosLogs");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "KudosLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KudosLogs_OrganizationId",
                table: "KudosLogs",
                column: "OrganizationId");
        }
    }
}
