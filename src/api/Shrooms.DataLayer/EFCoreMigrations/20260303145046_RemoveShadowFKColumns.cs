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
            // All DROP operations are guarded with IF EXISTS so this migration is safe on
            // brownfield databases where InitialBaseline was skipped and these shadow
            // columns/FKs were never created.
            // Order: drop FKs → drop indexes → drop columns → recreate clean index.
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BadgeCategoryKudosType_BadgeCategories_BadgeCategoryId1' AND parent_object_id = OBJECT_ID('BadgeCategoryKudosType'))
                    ALTER TABLE BadgeCategoryKudosType DROP CONSTRAINT FK_BadgeCategoryKudosType_BadgeCategories_BadgeCategoryId1;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BadgeTypes_BadgeCategories_BadgeCategoryId1' AND parent_object_id = OBJECT_ID('BadgeTypes'))
                    ALTER TABLE BadgeTypes DROP CONSTRAINT FK_BadgeTypes_BadgeCategories_BadgeCategoryId1;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Events_AspNetUsers_ResponsibleUserId1' AND parent_object_id = OBJECT_ID('Events'))
                    ALTER TABLE Events DROP CONSTRAINT FK_Events_AspNetUsers_ResponsibleUserId1;
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Events_ResponsibleUserId1' AND object_id = OBJECT_ID('Events'))
                    DROP INDEX IX_Events_ResponsibleUserId1 ON Events;
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BadgeTypes_BadgeCategoryId1' AND object_id = OBJECT_ID('BadgeTypes'))
                    DROP INDEX IX_BadgeTypes_BadgeCategoryId1 ON BadgeTypes;
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BadgeCategoryKudosType_BadgeCategoryId1' AND object_id = OBJECT_ID('BadgeCategoryKudosType'))
                    DROP INDEX IX_BadgeCategoryKudosType_BadgeCategoryId1 ON BadgeCategoryKudosType;
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BadgeCategoryKudosType_KudosTypeId' AND object_id = OBJECT_ID('BadgeCategoryKudosType'))
                    DROP INDEX IX_BadgeCategoryKudosType_KudosTypeId ON BadgeCategoryKudosType;
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Events' AND COLUMN_NAME = 'ResponsibleUserId1')
                    ALTER TABLE Events DROP COLUMN ResponsibleUserId1;
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BadgeTypes' AND COLUMN_NAME = 'BadgeCategoryId1')
                    ALTER TABLE BadgeTypes DROP COLUMN BadgeCategoryId1;
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BadgeCategoryKudosType' AND COLUMN_NAME = 'BadgeCategoryId1')
                    ALTER TABLE BadgeCategoryKudosType DROP COLUMN BadgeCategoryId1;
                IF OBJECT_ID('BadgeCategoryKudosType') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BadgeCategoryKudosType_KudosTypeId' AND object_id = OBJECT_ID('BadgeCategoryKudosType'))
                    CREATE INDEX IX_BadgeCategoryKudosType_KudosTypeId ON BadgeCategoryKudosType (KudosTypeId);
                -- Rename brownfield EF6 columns to match EF Core shadow property names
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Floors' AND COLUMN_NAME = 'Picture_Id')
                    EXEC sp_rename 'Floors.Picture_Id', 'PictureId1', 'COLUMN';
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NotificationsSettings' AND COLUMN_NAME = 'ApplicationUser_Id')
                    EXEC sp_rename 'NotificationsSettings.ApplicationUser_Id', 'ApplicationUserId', 'COLUMN';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NotificationsSettings' AND COLUMN_NAME = 'ApplicationUserId')
                    EXEC sp_rename 'NotificationsSettings.ApplicationUserId', 'ApplicationUser_Id', 'COLUMN';
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Floors' AND COLUMN_NAME = 'PictureId1')
                    EXEC sp_rename 'Floors.PictureId1', 'Picture_Id', 'COLUMN';
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Floors_Pictures_PictureId1",
                table: "Floors");

            migrationBuilder.DropIndex(
                name: "IX_BadgeCategoryKudosType_KudosTypeId",
                table: "BadgeCategoryKudosType");

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
