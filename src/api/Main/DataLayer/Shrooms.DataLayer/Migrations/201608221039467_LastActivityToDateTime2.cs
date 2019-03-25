namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class LastActivityToDateTime2 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Posts", "IX_LastCommentCreated");
            AlterColumn("dbo.Posts", "LastActivity", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            CreateIndex("dbo.Posts", "LastActivity");
        }

        public override void Down()
        {
            DropIndex("dbo.Posts", new[] { "LastActivity" });
            AlterColumn("dbo.Posts", "LastActivity", c => c.DateTime(nullable: false));
            CreateIndex("dbo.Posts", "LastActivity", false, "IX_LastCommentCreated");
        }
    }
}
