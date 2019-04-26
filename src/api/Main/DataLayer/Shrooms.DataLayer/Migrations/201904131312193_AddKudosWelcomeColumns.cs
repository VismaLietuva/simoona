namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddKudosWelcomeColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "KudosWelcomeAmount", c => c.Int(nullable: false));
            AddColumn("dbo.Organizations", "KudosWelcomeEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Organizations", "KudosWelcomeComment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organizations", "KudosWelcomeComment");
            DropColumn("dbo.Organizations", "KudosWelcomeEnabled");
            DropColumn("dbo.Organizations", "KudosWelcomeAmount");
        }
    }
}
