namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedNotificationFlagFromUser : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE [dbo].[WallMembers]
	                SET [EmailNotificationsEnabled] = 0
                FROM [dbo].[WallMembers] as member
                Inner Join [dbo].[AspNetUsers] as users
                On users.Id = member.UserId
                Inner Join [dbo].[Walls] as wall
                On member.WallId = wall.Id
                Where wall.[Type] = 1 and users.ShroomsInfoDisabled = 1
                GO");

            DropColumn("dbo.AspNetUsers", "ShroomsInfoDisabled");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "ShroomsInfoDisabled", c => c.Boolean(nullable: false));
        }
    }
}
