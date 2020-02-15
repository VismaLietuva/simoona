using System;
using System.IO;

namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MigrateLikeEntityToJsonFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "Likes", c => c.String(defaultValue: "{}"));
            AddColumn("dbo.Comments", "Likes", c => c.String(defaultValue: "{}"));
            SqlFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"sql_scripts\likes_migration.sql")); // creates ConverXmltToJson function
            Sql(@"UPDATE dbo.Comments SET Likes = (SELECT dbo.ConvertXmlToJson((SELECT LikeUserId as 'userId', Created as 'created' FROM Likes WHERE Likes.LikedCommentId = dbo.Comments.Id FOR XML PATH, ROOT)));");
            Sql(@"UPDATE dbo.Posts SET Likes = (SELECT dbo.ConvertXmlToJson((SELECT LikeUserId as 'userId', Created as 'created' FROM Likes WHERE Likes.LikedPostId = dbo.Posts.Id FOR XML PATH, ROOT)));");
            Sql("DROP FUNCTION dbo.ConvertXmlToJson;");
            Sql(@"UPDATE dbo.Posts SET Likes = '[]' WHERE Likes IS NULL");
            Sql(@"UPDATE dbo.Comments SET Likes = '[]' WHERE Likes IS NULL");
            DropForeignKey("dbo.Likes", "LikedCommentId", "dbo.Comments");
            DropForeignKey("dbo.Likes", "LikedPostId", "dbo.Posts");
            DropForeignKey("dbo.Likes", "LikeUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Likes", new[] { "LikeUserId" });
            DropIndex("dbo.Likes", new[] { "LikedCommentId" });
            DropIndex("dbo.Likes", new[] { "LikedPostId" });
            DropTable("dbo.Likes");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.Likes",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    LikeUserId = c.String(nullable: false, maxLength: 128),
                    LikedCommentId = c.Int(),
                    LikedPostId = c.Int(),
                    Created = c.DateTime(nullable: false),
                    CreatedBy = c.String(),
                    Modified = c.DateTime(nullable: false),
                    ModifiedBy = c.String(),
                    IsDeleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            DropColumn("dbo.Comments", "Likes");
            DropColumn("dbo.Posts", "Likes");
            CreateIndex("dbo.Likes", "LikedPostId");
            CreateIndex("dbo.Likes", "LikedCommentId");
            CreateIndex("dbo.Likes", "LikeUserId");
            AddForeignKey("dbo.Likes", "LikeUserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Likes", "LikedPostId", "dbo.Posts", "Id");
            AddForeignKey("dbo.Likes", "LikedCommentId", "dbo.Comments", "Id");
        }
    }
}
