namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHideFromUpcomingEventsWidgetPropertyToEvent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "HideFromUpcomingEventsWidget", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "HideFromUpcomingEventsWidget");
        }
    }
}
