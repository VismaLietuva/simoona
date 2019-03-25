namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsDeletedToUpvoteTable : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Upvotes", new[] { "UserId" });
            AddColumn("dbo.Upvotes", "IsDeleted", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Upvotes", "UserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Upvotes", "UserId");

            Sql(@"UPDATE[dbo].[Upvotes]
                    SET[IsDeleted] = 1
                    WHERE[Id] IN(SELECT Upvote.Id
                        FROM[dbo].[Upvotes] as upvote
                        Join[dbo].[Posts] as post
                        On upvote.PostId = post.Id
                        Where post.IsDeleted = 1)");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Upvotes", new[] { "UserId" });
            AlterColumn("dbo.Upvotes", "UserId", c => c.String(maxLength: 128));
            DropColumn("dbo.Upvotes", "IsDeleted");
            CreateIndex("dbo.Upvotes", "UserId");
        }
    }
}
