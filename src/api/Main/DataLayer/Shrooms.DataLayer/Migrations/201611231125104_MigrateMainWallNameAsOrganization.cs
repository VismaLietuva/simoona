namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MigrateMainWallNameAsOrganization : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE dbo.Walls
                  SET Name = Organizations.Name
                  FROM dbo.Walls AS Walls
                  JOIN dbo.Organizations AS Organizations ON Walls.OrganizationId = Organizations.Id
                  WHERE Walls.Type = 0");
        }
    }
}
