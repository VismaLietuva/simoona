using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <inheritdoc />
    public partial class RoundUserKudosBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [dbo].[AspNetUsers]
SET [TotalKudos] = ROUND([TotalKudos], 0),
    [SpentKudos] = ROUND([SpentKudos], 0),
    [RemainingKudos] = ROUND([TotalKudos], 0) - ROUND([SpentKudos], 0)
WHERE [TotalKudos] <> ROUND([TotalKudos], 0)
   OR [SpentKudos] <> ROUND([SpentKudos], 0)
   OR [RemainingKudos] <> ROUND([RemainingKudos], 0);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
