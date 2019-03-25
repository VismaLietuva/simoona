namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LastEditFieldsToPostAndCommentEntities : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "LastEdit", c => c.DateTime(nullable: false));
            AddColumn("dbo.Comments", "LastEdit", c => c.DateTime(nullable: false));            
        }
        
        public override void Down()
        {
            DropColumn("dbo.Comments", "LastEdit");
            DropColumn("dbo.Posts", "LastEdit");            
        }
    }
}
