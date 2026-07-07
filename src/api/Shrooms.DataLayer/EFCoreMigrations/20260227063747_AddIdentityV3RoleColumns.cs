using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <summary>
    /// Adds ASP.NET Core Identity v3 columns missing from AspNetRoles in the old Identity v2 schema.
    /// Idempotent - safe to apply to both brownfield and fresh installs.
    /// </summary>
    public partial class AddIdentityV3RoleColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns idempotently (IF NOT EXISTS guard)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetRoles' AND COLUMN_NAME = 'NormalizedName')
                    ALTER TABLE AspNetRoles ADD NormalizedName NVARCHAR(256) NULL;
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetRoles' AND COLUMN_NAME = 'ConcurrencyStamp')
                    ALTER TABLE AspNetRoles ADD ConcurrencyStamp NVARCHAR(MAX) NULL;
            ");
            // Populate using dynamic SQL so SQL Server does not fail to compile the batch
            // when the columns did not exist at parse time (separate execution batch).
            migrationBuilder.Sql(@"
                EXEC sp_executesql N'UPDATE AspNetRoles SET
                    NormalizedName   = COALESCE(NormalizedName, UPPER(Name)),
                    ConcurrencyStamp = COALESCE(ConcurrencyStamp, CAST(NEWID() AS NVARCHAR(MAX)))';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "NormalizedName", table: "AspNetRoles");
            migrationBuilder.DropColumn(name: "ConcurrencyStamp", table: "AspNetRoles");
        }
    }
}
