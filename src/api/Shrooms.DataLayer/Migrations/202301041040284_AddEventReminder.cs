namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEventReminder : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventReminders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventId = c.Guid(nullable: false),
                        RemindBeforeInDays = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Reminded = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.EventId)
                .Index(t => t.EventId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EventReminders", "EventId", "dbo.Events");
            DropIndex("dbo.EventReminders", new[] { "EventId" });
            DropTable("dbo.EventReminders");
        }
    }
}
