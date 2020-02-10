namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsShownWithAllEventsField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventTypes", "IsShownWithAllEvents", 
                c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventTypes", "IsShownWithAllEvents");
        }
    }
}
