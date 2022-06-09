namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAddForNewUsersProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Walls", "AddForNewUsers", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Walls", "AddForNewUsers");
        }
    }
}
