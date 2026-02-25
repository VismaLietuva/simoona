using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shrooms.DataLayer.DAL
{
    /// <summary>
    /// Design-time factory used by EF Core tools (dotnet ef migrations add/update).
    /// The connection string here is only used at design time; runtime uses appsettings.json.
    /// </summary>
    public class ShroomsDbContextFactory : IDesignTimeDbContextFactory<ShroomsDbContext>
    {
        public ShroomsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ShroomsDbContext>();
            optionsBuilder.UseSqlServer(
                "Data Source=127.0.0.1,1434;User ID=sa;Password=Password!123;MultipleActiveResultSets=True;TrustServerCertificate=True;Database=SimoonaDB;");

            return new ShroomsDbContext(optionsBuilder.Options);
        }
    }
}
