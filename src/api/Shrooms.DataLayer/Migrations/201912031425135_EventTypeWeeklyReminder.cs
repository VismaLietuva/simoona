namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EventTypeWeeklyReminder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventTypes", "SendWeeklyReminders", c => c.Boolean(nullable: false));
            AddColumn("dbo.NotificationsSettings", "EventWeeklyReminderAppNotifications", c => c.Boolean(nullable: false));
            AddColumn("dbo.NotificationsSettings", "EventWeeklyReminderEmailNotifications", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NotificationsSettings", "EventWeeklyReminderEmailNotifications");
            DropColumn("dbo.NotificationsSettings", "EventWeeklyReminderAppNotifications");
            DropColumn("dbo.EventTypes", "SendWeeklyReminders");
        }
    }
}
