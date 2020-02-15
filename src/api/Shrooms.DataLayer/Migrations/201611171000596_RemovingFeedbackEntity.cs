namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovingFeedbackEntity : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Feedbacks", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Feedbacks", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.Feedbacks", new[] { "ApplicationUserId" });
            DropIndex("dbo.Feedbacks", new[] { "OrganizationId" });
            DropTable("dbo.Feedbacks");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Subject = c.String(),
                        Message = c.String(),
                        Email = c.String(),
                        ApplicationUserId = c.String(maxLength: 128),
                        IsDone = c.Boolean(nullable: false),
                        DoneTime = c.DateTime(),
                        FeedbackType = c.Int(nullable: false),
                        OrganizationId = c.Int(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Feedbacks", "OrganizationId");
            CreateIndex("dbo.Feedbacks", "ApplicationUserId");
            AddForeignKey("dbo.Feedbacks", "OrganizationId", "dbo.Organizations", "Id");
            AddForeignKey("dbo.Feedbacks", "ApplicationUserId", "dbo.AspNetUsers", "Id");
        }
    }
}
