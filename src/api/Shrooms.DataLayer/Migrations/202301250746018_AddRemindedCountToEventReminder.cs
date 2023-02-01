namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRemindedCountToEventReminder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventReminders", "IsReminded", c => c.Boolean(nullable: false));
            AddColumn("dbo.EventReminders", "RemindedCount", c => c.Int(nullable: false));
            DropColumn("dbo.EventReminders", "Reminded");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EventReminders", "Reminded", c => c.Boolean(nullable: false));
            DropColumn("dbo.EventReminders", "RemindedCount");
            DropColumn("dbo.EventReminders", "IsReminded");
        }
    }
}
