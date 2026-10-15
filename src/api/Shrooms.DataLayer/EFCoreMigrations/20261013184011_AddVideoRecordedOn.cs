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
            migrationBuilder.AddColumn<DateOnly>(
                name: "RecordedOn",
                table: "VideoLibraryItems",
                type: "date",
                nullable: true);

            migrationBuilder.Sql("UPDATE [VideoLibraryItems] SET [RecordedOn] = CAST([Created] AS date)");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "RecordedOn",
                table: "VideoLibraryItems",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

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
