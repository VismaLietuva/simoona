namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsDeletedToEventNotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventNotifications", "IsDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventNotifications", "IsDeleted");
        }
    }
}
