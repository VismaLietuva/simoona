using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ChangingNotifiationSourceToSerializedObject : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "Sources", c => c.String());
            Sql("UPDATE dbo.Notifications SET Sources = '{\"postId\":\"'+SourceId+'\"}' WHERE Type = " + (int)NotificationType.WallPost);
            Sql("UPDATE dbo.Notifications SET Sources = '{\"eventId\":\"'+SourceId+'\"}' WHERE Type = " + (int)NotificationType.NewEvent);
            DropColumn("dbo.Notifications", "SourceId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "SourceId", c => c.String());
            DropColumn("dbo.Notifications", "Sources");
        }
    }
}
