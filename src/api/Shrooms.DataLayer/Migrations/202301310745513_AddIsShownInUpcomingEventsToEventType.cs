namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsShownInUpcomingEventsToEventType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventTypes", "IsShownInUpcomingEvents", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventTypes", "IsShownInUpcomingEvents");
        }
    }
}
