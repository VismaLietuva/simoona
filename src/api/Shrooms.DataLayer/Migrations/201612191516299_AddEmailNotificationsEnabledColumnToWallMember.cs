namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEmailNotificationsEnabledColumnToWallMember : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WallMembers", "EmailNotificationsEnabled", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WallMembers", "EmailNotificationsEnabled");
        }
    }
}
