using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    public partial class AddProviderDisplayName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The AddMissingIdentityTables migration only creates AspNetUserLogins when the
            // table does not exist. Brownfield databases already had the table from the old
            // .NET Framework Identity schema, which lacked ProviderDisplayName. Add it here.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'AspNetUserLogins' AND COLUMN_NAME = 'ProviderDisplayName'
                )
                    ALTER TABLE dbo.AspNetUserLogins ADD ProviderDisplayName NVARCHAR(MAX) NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ProviderDisplayName", table: "AspNetUserLogins");
        }
    }
}
