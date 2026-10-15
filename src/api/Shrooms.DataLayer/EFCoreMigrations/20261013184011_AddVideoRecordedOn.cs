using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class AddVideoRecordedOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RecordedOn",
                table: "VideoLibraryItems",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("UPDATE [VideoLibraryItems] SET [RecordedOn] = CAST([Created] AS date)");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLibraryItems_OrganizationId_RecordedOn",
                table: "VideoLibraryItems",
                columns: new[] { "OrganizationId", "RecordedOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoLibraryItems_OrganizationId_RecordedOn",
                table: "VideoLibraryItems");

            migrationBuilder.DropColumn(
                name: "RecordedOn",
                table: "VideoLibraryItems");
        }
    }
}
