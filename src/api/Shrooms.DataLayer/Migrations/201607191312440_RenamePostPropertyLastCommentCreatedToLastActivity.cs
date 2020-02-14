namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RenamePostPropertyLastCommentCreatedToLastActivity : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Posts", "LastCommentCreated", "LastActivity");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Posts", "LastActivity", "LastCommentCreated");
        }
    }
}
