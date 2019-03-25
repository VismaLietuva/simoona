namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MigrateSimoonaWallName : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE dbo.Walls SET Name = 'Visma Lietuva' WHERE Name = 'Simoona'");
        }
    }
}
