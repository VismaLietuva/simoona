namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTableUpvote : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Upvotes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OriginWallId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        PostId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Walls", t => t.OriginWallId, cascadeDelete: true)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.OriginWallId)
                .Index(t => t.UserId)
                .Index(t => t.PostId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Upvotes", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Upvotes", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Upvotes", "OriginWallId", "dbo.Walls");
            DropIndex("dbo.Upvotes", new[] { "PostId" });
            DropIndex("dbo.Upvotes", new[] { "UserId" });
            DropIndex("dbo.Upvotes", new[] { "OriginWallId" });
            DropTable("dbo.Upvotes");
        }
    }
}
