namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAppEmailNotificationsColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WallMembers", "AppNotificationsEnabled", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WallMembers", "AppNotificationsEnabled");
        }
    }
}
