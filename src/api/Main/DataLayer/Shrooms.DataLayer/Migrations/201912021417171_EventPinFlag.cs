namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EventPinFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "IsPinned", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "IsPinned");
        }
    }
}
