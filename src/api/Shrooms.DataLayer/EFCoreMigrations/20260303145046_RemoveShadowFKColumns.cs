using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class RemoveShadowFKColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BadgeCategoryKudosType_BadgeCategories_BadgeCategoryId1",
                table: "BadgeCategoryKudosType");

            migrationBuilder.DropForeignKey(
                name: "FK_BadgeTypes_BadgeCategories_BadgeCategoryId1",
                table: "BadgeTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_AspNetUsers_ResponsibleUserId1",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ResponsibleUserId1",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_BadgeTypes_BadgeCategoryId1",
                table: "BadgeTypes");

            migrationBuilder.DropIndex(
                name: "IX_BadgeCategoryKudosType_BadgeCategoryId1",
                table: "BadgeCategoryKudosType");

            migrationBuilder.DropIndex(
                name: "IX_BadgeCategoryKudosType_KudosTypeId",
                table: "BadgeCategoryKudosType");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserId1",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "BadgeCategoryId1",
                table: "BadgeTypes");

            migrationBuilder.DropColumn(
                name: "BadgeCategoryId1",
                table: "BadgeCategoryKudosType");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeCategoryKudosType_KudosTypeId",
                table: "BadgeCategoryKudosType",
                column: "KudosTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Floors_Pictures_PictureId1",
                table: "Floors");

            migrationBuilder.DropIndex(
                name: "IX_BadgeCategoryKudosType_KudosTypeId",
                table: "BadgeCategoryKudosType");

            migrationBuilder.RenameColumn(
                name: "PictureId1",
                table: "Floors",
                newName: "Picture_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Floors_PictureId1",
                table: "Floors",
                newName: "IX_Floors_Picture_Id");

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleUserId1",
                table: "Events",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BadgeCategoryId1",
                table: "BadgeTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BadgeCategoryId1",
                table: "BadgeCategoryKudosType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ResponsibleUserId1",
                table: "Events",
                column: "ResponsibleUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeTypes_BadgeCategoryId1",
                table: "BadgeTypes",
                column: "BadgeCategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeCategoryKudosType_BadgeCategoryId1",
                table: "BadgeCategoryKudosType",
                column: "BadgeCategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeCategoryKudosType_KudosTypeId",
                table: "BadgeCategoryKudosType",
                column: "KudosTypeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BadgeCategoryKudosType_BadgeCategories_BadgeCategoryId1",
                table: "BadgeCategoryKudosType",
                column: "BadgeCategoryId1",
                principalTable: "BadgeCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BadgeTypes_BadgeCategories_BadgeCategoryId1",
                table: "BadgeTypes",
                column: "BadgeCategoryId1",
                principalTable: "BadgeCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_AspNetUsers_ResponsibleUserId1",
                table: "Events",
                column: "ResponsibleUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Floors_Pictures_Picture_Id",
                table: "Floors",
                column: "Picture_Id",
                principalTable: "Pictures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
