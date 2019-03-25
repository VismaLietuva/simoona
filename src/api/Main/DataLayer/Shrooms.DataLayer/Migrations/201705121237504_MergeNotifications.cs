namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MergeNotifications : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.NotificationApplicationUsers", "Notification_Id", "dbo.Notifications");
            DropForeignKey("dbo.NotificationApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.NotificationApplicationUsers", new[] { "Notification_Id" });
            DropIndex("dbo.NotificationApplicationUsers", new[] { "ApplicationUser_Id" });
            CreateTable(
                "dbo.NotificationUsers",
                c => new
                    {
                        NotificationId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        IsAlreadySeen = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.NotificationId, t.UserId })
                .ForeignKey("dbo.Notifications", t => t.NotificationId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.NotificationId)
                .Index(t => t.UserId)
                .Index(t => t.IsAlreadySeen, name: "ix_notification_IsAlreadySeen");
            
            AddColumn("dbo.Notifications", "SourceId", c => c.String());
            DropTable("dbo.NotificationApplicationUsers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.NotificationApplicationUsers",
                c => new
                    {
                        Notification_Id = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Notification_Id, t.ApplicationUser_Id });
            
            DropForeignKey("dbo.NotificationUsers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NotificationUsers", "NotificationId", "dbo.Notifications");
            DropIndex("dbo.NotificationUsers", "ix_notification_IsAlreadySeen");
            DropIndex("dbo.NotificationUsers", new[] { "UserId" });
            DropIndex("dbo.NotificationUsers", new[] { "NotificationId" });
            DropColumn("dbo.Notifications", "SourceId");
            DropTable("dbo.NotificationUsers");
            CreateIndex("dbo.NotificationApplicationUsers", "ApplicationUser_Id");
            CreateIndex("dbo.NotificationApplicationUsers", "Notification_Id");
            AddForeignKey("dbo.NotificationApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.NotificationApplicationUsers", "Notification_Id", "dbo.Notifications", "Id", cascadeDelete: true);
        }
    }
}
