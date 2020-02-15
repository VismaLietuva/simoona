namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedUpvote : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Upvotes", "OriginWallId", "dbo.Walls");
            DropForeignKey("dbo.Upvotes", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Upvotes", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Upvotes", new[] { "OriginWallId" });
            DropIndex("dbo.Upvotes", new[] { "UserId" });
            DropIndex("dbo.Upvotes", new[] { "PostId" });
            DropTable("dbo.Upvotes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Upvotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OriginWallId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        PostId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Upvotes", "PostId");
            CreateIndex("dbo.Upvotes", "UserId");
            CreateIndex("dbo.Upvotes", "OriginWallId");
            AddForeignKey("dbo.Upvotes", "UserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Upvotes", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Upvotes", "OriginWallId", "dbo.Walls", "Id", cascadeDelete: true);
        }
    }
}
