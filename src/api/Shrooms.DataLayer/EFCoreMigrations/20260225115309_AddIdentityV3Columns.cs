using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <summary>
    /// Adds ASP.NET Core Identity v3 columns missing from the old Identity v2 schema (created by
    /// EF6 migrations). Uses idempotent SQL so this migration is safe to apply to both:
    /// - Existing EF6-migrated databases (brownfield): adds the missing columns
    /// - Fresh installs: columns already created by InitialBaseline, IF NOT EXISTS guards skip them
    /// </summary>
    public partial class AddIdentityV3Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Batch 1: DDL — add missing Identity v3 columns if not present.
            // Must be a separate Sql() call from the UPDATE below; SQL Server compiles
            // each batch before executing it, so referencing a just-added column in the
            // same batch causes "Invalid column name" even if the ADD runs first.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'NormalizedUserName')
                    ALTER TABLE AspNetUsers ADD NormalizedUserName NVARCHAR(256) NULL;
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'NormalizedEmail')
                    ALTER TABLE AspNetUsers ADD NormalizedEmail NVARCHAR(256) NULL;
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'ConcurrencyStamp')
                    ALTER TABLE AspNetUsers ADD ConcurrencyStamp NVARCHAR(MAX) NULL;
            ");

            // Batch 2: DML — back-fill values and create index (columns guaranteed to exist now).
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers SET
                    NormalizedUserName = COALESCE(NormalizedUserName, UPPER(UserName)),
                    NormalizedEmail    = COALESCE(NormalizedEmail, UPPER(Email)),
                    ConcurrencyStamp   = COALESCE(ConcurrencyStamp, CAST(NEWID() AS NVARCHAR(MAX)));
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UserNameIndex' AND object_id = OBJECT_ID('AspNetUsers'))
                    CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "NormalizedUserName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "NormalizedEmail", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "ConcurrencyStamp", table: "AspNetUsers");
        }
    }
}
