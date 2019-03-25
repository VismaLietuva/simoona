namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotificationsUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "Type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "Type");
        }
    }
}