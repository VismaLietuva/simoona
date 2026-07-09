using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddKudosLogLikes : Migration
    {
        // Note: the accompanying model snapshot also picks up IsDeleted columns on
        // BadgeTypes/BadgeLogs/BadgeCategories that the previous snapshot was missing.
        // Those columns already exist in the database (created by InitialBaseline),
        // so no schema operations are emitted for them here — only the snapshot is
        // brought back in sync.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Likes",
                table: "KudosLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Likes",
                table: "KudosLogs");
        }
    }
}
