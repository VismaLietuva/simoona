namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBookCoverUrlToBook : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Books", "BookCoverUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Books", "BookCoverUrl");
        }
    }
}
