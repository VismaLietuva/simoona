using System.Data.Entity.Migrations;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.Migrations.DataInitializer;

namespace Shrooms.DataLayer.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<ShroomsDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "Shrooms.DataLayer.ShroomsDbContext";
            CommandTimeout = 60 * 5;

            SetSqlGenerator("System.Data.SqlClient", new DefaultValueSqlServerMigrationSqlGenerator());

        }

        protected override void Seed(ShroomsDbContext context)
        {
            RoleInitializer.CreateRoles(context);
            ModulesInitializer.Create(context);
            PermissionInitializer.CreatePermissions(context);
        }
    }
}