namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveEventCommentsTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EventComments", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EventComments", "EventId", "dbo.Events");
            DropIndex("dbo.EventComments", new[] { "EventId" });
            DropIndex("dbo.EventComments", new[] { "ApplicationUserId" });
            DropTable("dbo.EventComments");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.EventComments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventId = c.Guid(nullable: false),
                        Comment = c.String(nullable: false),
                        ApplicationUserId = c.String(nullable: false, maxLength: 128),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.EventComments", "ApplicationUserId");
            CreateIndex("dbo.EventComments", "EventId");
            AddForeignKey("dbo.EventComments", "EventId", "dbo.Events", "Id");
            AddForeignKey("dbo.EventComments", "ApplicationUserId", "dbo.AspNetUsers", "Id");
        }
    }
}
