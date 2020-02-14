namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCommentsNotificationsSettings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationsSettings", "MyPostsAppNotifications", c => c.Boolean(nullable: false));
            AddColumn("dbo.NotificationsSettings", "MyPostsEmailNotifications", c => c.Boolean(nullable: false));
            AddColumn("dbo.NotificationsSettings", "FollowingPostsAppNotifications", c => c.Boolean(nullable: false));
            AddColumn("dbo.NotificationsSettings", "FollowingPostsEmailNotifications", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NotificationsSettings", "FollowingPostsEmailNotifications");
            DropColumn("dbo.NotificationsSettings", "FollowingPostsAppNotifications");
            DropColumn("dbo.NotificationsSettings", "MyPostsEmailNotifications");
            DropColumn("dbo.NotificationsSettings", "MyPostsAppNotifications");
        }
    }
}
