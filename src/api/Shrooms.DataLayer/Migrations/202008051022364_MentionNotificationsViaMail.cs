namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class MentionNotificationsViaMail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationsSettings", "MentionEmailNotifications", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NotificationsSettings", "MentionEmailNotifications");
        }
    }
}
