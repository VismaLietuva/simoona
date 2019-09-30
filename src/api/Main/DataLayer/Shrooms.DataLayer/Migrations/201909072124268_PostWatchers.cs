namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PostWatchers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PostWatchers",
                c => new
                    {
                        PostId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.PostId, t.UserId }, "PK_PostWatchers")
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.UserId);

            Sql("INSERT INTO dbo.PostWatchers (PostId, UserId) " +
                "SELECT DISTINCT PostId,AuthorId FROM dbo.Comments WHERE AuthorId IS NOT NULL" +
                " UNION " +
                "SELECT DISTINCT Id, AuthorId FROM dbo.Posts");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PostWatchers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PostWatchers", "PostId", "dbo.Posts");
            DropIndex("dbo.PostWatchers", new[] { "UserId" });
            DropIndex("dbo.PostWatchers", new[] { "PostId" });
            DropTable("dbo.PostWatchers");
        }
    }
}
