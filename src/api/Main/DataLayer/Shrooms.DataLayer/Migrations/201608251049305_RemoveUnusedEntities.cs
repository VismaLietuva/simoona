namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUnusedEntities : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SubscribedPosts", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.SubscribedPosts", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.SubscribedPosts", "PostId", "dbo.Posts");
            DropForeignKey("dbo.WantedCertificates", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.WantedCertificateApplicationUsers", "WantedCertificate_Id", "dbo.WantedCertificates");
            DropForeignKey("dbo.WantedCertificateApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.SubscribedPosts", new[] { "PostId" });
            DropIndex("dbo.SubscribedPosts", new[] { "ApplicationUserId" });
            DropIndex("dbo.SubscribedPosts", new[] { "OrganizationId" });
            DropIndex("dbo.WantedCertificates", new[] { "OrganizationId" });
            DropIndex("dbo.WantedCertificateApplicationUsers", new[] { "WantedCertificate_Id" });
            DropIndex("dbo.WantedCertificateApplicationUsers", new[] { "ApplicationUser_Id" });
            DropTable("dbo.SubscribedPosts");
            DropTable("dbo.WantedCertificates");
            DropTable("dbo.WantedCertificateApplicationUsers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.WantedCertificateApplicationUsers",
                c => new
                    {
                        WantedCertificate_Id = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.WantedCertificate_Id, t.ApplicationUser_Id });
            
            CreateTable(
                "dbo.WantedCertificates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        OrganizationId = c.Int(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SubscribedPosts",
                c => new
                    {
                        PostId = c.Int(nullable: false),
                        ApplicationUserId = c.String(nullable: false, maxLength: 128),
                        Id = c.Int(nullable: false, identity: true),
                        Subscribed = c.Boolean(nullable: false),
                        OrganizationId = c.Int(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.PostId, t.ApplicationUserId });
            
            CreateIndex("dbo.WantedCertificateApplicationUsers", "ApplicationUser_Id");
            CreateIndex("dbo.WantedCertificateApplicationUsers", "WantedCertificate_Id");
            CreateIndex("dbo.WantedCertificates", "OrganizationId");
            CreateIndex("dbo.SubscribedPosts", "OrganizationId");
            CreateIndex("dbo.SubscribedPosts", "ApplicationUserId");
            CreateIndex("dbo.SubscribedPosts", "PostId");
            AddForeignKey("dbo.WantedCertificateApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.WantedCertificateApplicationUsers", "WantedCertificate_Id", "dbo.WantedCertificates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.WantedCertificates", "OrganizationId", "dbo.Organizations", "Id");
            AddForeignKey("dbo.SubscribedPosts", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.SubscribedPosts", "OrganizationId", "dbo.Organizations", "Id");
            AddForeignKey("dbo.SubscribedPosts", "ApplicationUserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
