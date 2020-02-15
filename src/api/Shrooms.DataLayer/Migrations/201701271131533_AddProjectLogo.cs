namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddProjectLogo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "Logo", x => x.String());
            AlterColumn("dbo.Walls", "Logo", x => x.String(nullable: true));
        }

        public override void Down()
        {
            DropColumn("dbo.Projects", "Logo");
            AlterColumn("dbo.Walls", "Logo", x => x.String(nullable: false));
        }
    }
}
