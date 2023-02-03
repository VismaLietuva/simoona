namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeEventTypeAndEventWidgetPropertyNames : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "IsShownInUpcomingEventsWidget", c => c.Boolean(nullable: false));
            AddColumn("dbo.EventTypes", "CanBeDisplayedInUpcomingEventsWidget", c => c.Boolean(nullable: false));
            DropColumn("dbo.Events", "HideFromUpcomingEventsWidget");
            DropColumn("dbo.EventTypes", "IsShownInUpcomingEvents");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EventTypes", "IsShownInUpcomingEvents", c => c.Boolean(nullable: false));
            AddColumn("dbo.Events", "HideFromUpcomingEventsWidget", c => c.Boolean(nullable: false));
            DropColumn("dbo.EventTypes", "CanBeDisplayedInUpcomingEventsWidget");
            DropColumn("dbo.Events", "IsShownInUpcomingEventsWidget");
        }
    }
}
