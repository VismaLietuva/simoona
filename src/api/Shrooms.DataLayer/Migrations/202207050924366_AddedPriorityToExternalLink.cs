namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPriorityToExternalLink : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExternalLinks", "Priority", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExternalLinks", "Priority");
        }
    }
}
