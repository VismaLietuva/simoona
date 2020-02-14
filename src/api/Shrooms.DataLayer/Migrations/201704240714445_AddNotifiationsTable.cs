namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotifiationsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        PictureId = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId);
            
            CreateTable(
                "dbo.NotificationApplicationUsers",
                c => new
                    {
                        Notification_Id = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Notification_Id, t.ApplicationUser_Id })
                .ForeignKey("dbo.Notifications", t => t.Notification_Id, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .Index(t => t.Notification_Id)
                .Index(t => t.ApplicationUser_Id);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.NotificationApplicationUsers", "Notification_Id", "dbo.Notifications");
            DropForeignKey("dbo.Notifications", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.NotificationApplicationUsers", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.NotificationApplicationUsers", new[] { "Notification_Id" });
            DropIndex("dbo.Notifications", new[] { "OrganizationId" });
            DropTable("dbo.NotificationApplicationUsers");
            DropTable("dbo.Notifications");
        }
    }
}
