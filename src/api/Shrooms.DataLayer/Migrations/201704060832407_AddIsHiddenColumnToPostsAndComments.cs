namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsHiddenColumnToPostsAndComments : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "IsHidden", c => c.Boolean(nullable: false));
            AddColumn("dbo.Comments", "IsHidden", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Comments", "IsHidden");
            DropColumn("dbo.Posts", "IsHidden");
        }
    }
}
