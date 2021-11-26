namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedExternalLinkTypes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExternalLinks", "Type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExternalLinks", "Type");
        }
    }
}
