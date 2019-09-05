using System.Data.Entity.Migrations;

namespace Shrooms.DataLayer.Migrations
{
    public partial class PostWatchers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PostWatchers",
                c => new
                {
                    PostId = c.Int(nullable: false),
                    UserId = c.Guid(nullable: false)
                })
                .PrimaryKey(pk => new { pk.PostId, pk.UserId }, "PK_PostWatchers")
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true);

            Sql("insert into dbo.PostWatchers (PostId,UserId) " +
                "select distinct postid,authorid from dbo.Comments " +
                " union " +
                "select distinct id, AuthorId from dbo.Posts");
        }

        public override void Down()
        {
            DropTable("dbo.PostWatchers");
        }
    }
}
