using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <summary>
    /// Makes account lockout work on databases created by the legacy ASP.NET Identity 2 schema:
    /// 1. LockoutEndDateUtc is a datetime column there, while the EF Core model (and the snapshot) declare
    ///    datetimeoffset. Reading any user whose lockout end is set therefore threw an InvalidCastException,
    ///    which would have turned a locked account into one that can never sign in again. Converting the
    ///    column is lossless: existing values are UTC and become offset +00:00.
    /// 2. Identity only counts failed sign-ins for users with LockoutEnabled = 1, and most accounts were
    ///    created before lockout was configured, so the flag is switched on for everyone.
    /// </summary>
    public partial class EnableLockoutForAllUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'LockoutEndDateUtc' AND DATA_TYPE = 'datetime')
    ALTER TABLE [dbo].[AspNetUsers] ALTER COLUMN [LockoutEndDateUtc] datetimeoffset NULL;");

            migrationBuilder.Sql("UPDATE [dbo].[AspNetUsers] SET [LockoutEnabled] = 1 WHERE [LockoutEnabled] = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [dbo].[AspNetUsers] ALTER COLUMN [LockoutEndDateUtc] datetime NULL");
            // LockoutEnabled is left as is: there is no record of which users had it disabled before.
        }
    }
}
