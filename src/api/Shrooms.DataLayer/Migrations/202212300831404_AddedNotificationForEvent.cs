namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedNotificationForEvent : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventNotifications",
                c => new
                    {
                        EventId = c.Guid(nullable: false),
                        RemindBeforeEventStartInDays = c.Int(nullable: false),
                        EventStartNotified = c.Boolean(nullable: false),
                        RemindBeforeEventRegistrationDeadlineInDays = c.Int(nullable: false),
                        EventRegistrationDeadlineNotified = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.Events", t => t.EventId)
                .Index(t => t.EventId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EventNotifications", "EventId", "dbo.Events");
            DropIndex("dbo.EventNotifications", new[] { "EventId" });
            DropTable("dbo.EventNotifications");
        }
    }
}
