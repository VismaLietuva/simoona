namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFKtoNotificationsSettings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NotificationsSettings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventsAppNotifications = c.Boolean(nullable: false),
                        EventsEmailNotifications = c.Boolean(nullable: false),
                        ProjectsAppNotifications = c.Boolean(nullable: false),
                        ProjectsEmailNotifications = c.Boolean(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.OrganizationId)
                .Index(t => t.ApplicationUser_Id);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationsSettings", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.NotificationsSettings", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.NotificationsSettings", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.NotificationsSettings", new[] { "OrganizationId" });
            DropTable("dbo.NotificationsSettings");
        }
    }
}
