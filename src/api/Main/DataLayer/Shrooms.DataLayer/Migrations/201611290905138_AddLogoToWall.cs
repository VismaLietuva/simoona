namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddLogoToWall : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Walls", "Logo", c => c.String(nullable: false, defaultValueSql: "'wall-default.png'"));
        }

        public override void Down()
        {
            DropColumn("dbo.Walls", "Logo");
        }
    }
}
