using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    public partial class RenameOfficeAddressColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Brownfield EF6 databases stored Address as a complex type using the naming
            // convention Address_Country, Address_City, etc. EF Core's OwnsOne maps them
            // to bare column names (Country, City, ...). Rename only when the old names exist.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Offices' AND COLUMN_NAME = 'Address_Country')
BEGIN
    EXEC sp_rename 'dbo.Offices.Address_Country',  'Country',  'COLUMN'
    EXEC sp_rename 'dbo.Offices.Address_City',     'City',     'COLUMN'
    EXEC sp_rename 'dbo.Offices.Address_Street',   'Street',   'COLUMN'
    EXEC sp_rename 'dbo.Offices.Address_Building', 'Building', 'COLUMN'
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only rename back on databases where Up actually ran (brownfield).
            // Fresh databases never had Address_ columns so this is a no-op there.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Offices' AND COLUMN_NAME = 'Address_Country')
   AND EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260225115301_InitialBaseline'
               AND ProductVersion <> '10.0.0')
BEGIN
    EXEC sp_rename 'dbo.Offices.Country',  'Address_Country',  'COLUMN'
    EXEC sp_rename 'dbo.Offices.City',     'Address_City',     'COLUMN'
    EXEC sp_rename 'dbo.Offices.Street',   'Address_Street',   'COLUMN'
    EXEC sp_rename 'dbo.Offices.Building', 'Address_Building', 'COLUMN'
END
");
        }
    }
}
