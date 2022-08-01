namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCreatedLotterySettingToNotifications : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationsSettings", "CreatedLotteryEmailNotifications", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NotificationsSettings", "CreatedLotteryEmailNotifications");
        }
    }
}
