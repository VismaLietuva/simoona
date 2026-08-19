using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddKudosLogSentTo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SentToId",
                table: "KudosLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KudosLogs_SentToId",
                table: "KudosLogs",
                column: "SentToId");

            migrationBuilder.AddForeignKey(
                name: "FK_KudosLogs_AspNetUsers_SentToId",
                table: "KudosLogs",
                column: "SentToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KudosLogs_AspNetUsers_SentToId",
                table: "KudosLogs");

            migrationBuilder.DropIndex(
                name: "IX_KudosLogs_SentToId",
                table: "KudosLogs");

            migrationBuilder.DropColumn(
                name: "SentToId",
                table: "KudosLogs");
        }
    }
}
