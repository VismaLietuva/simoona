namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInverseEventTypeUpcomingEventsWidgetDisplaySettingToEvent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "InverseEventTypeUpcomingEventsWidgetDisplaySetting", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "InverseEventTypeUpcomingEventsWidgetDisplaySetting");
        }
    }
}
