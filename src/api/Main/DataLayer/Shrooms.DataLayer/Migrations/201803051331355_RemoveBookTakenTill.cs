namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveBookTakenTill : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.BookLogs", "TakenTill");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BookLogs", "TakenTill", c => c.DateTime(nullable: false));
        }
    }
}
