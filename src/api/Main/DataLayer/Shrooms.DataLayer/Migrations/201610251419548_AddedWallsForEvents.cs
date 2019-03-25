using System.Data.Entity.Migrations;
using Shrooms.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.Migrations
{
    public partial class AddedWallsForEvents : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "WallId", c => c.Int());
            AddColumn("dbo.Walls", "Eventid", c => c.Guid());
            CreateIndex("dbo.Events", "WallId");
            AddForeignKey("dbo.Events", "WallId", "dbo.Walls", "Id");

            //Setting wall type for current walls
            Sql("UPDATE [dbo].[Walls]" +
                "SET[Type] =" + (int)WallType.UserCreated +
                "WHERE[Name] != 'Simoona'");

            //Insert wall for every event
            Sql(@"INSERT INTO [dbo].[Walls]
                    ([Name]
                    ,[Description]
                    ,[OrganizationId]
                    ,[Created]
                    ,[CreatedBy]
                    ,[Modified]
                    ,[ModifiedBy]
                    ,[IsDeleted]
                    ,[Type]
                    ,[Access]
		            ,[EventId])
                SELECT
			        SimoonaEvent.Name,
			        null,
			        SimoonaEvent.OrganizationId,
			        SimoonaEvent.Created,
			        SimoonaEvent.CreatedBy,
			        SimoonaEvent.Modified,
			        SimoonaEvent.ModifiedBy,
			        SimoonaEvent.IsDeleted," +
                    (int)WallType.Events + "," +
                    (int)WallAccess.Private + "," +
                    @"SimoonaEvent.Id
	            From [dbo].[Events] as SimoonaEvent");

            //Link walls to events
            Sql(@"UPDATE [dbo].[Events]
                SET
                    [WallId] = walls.Id
                FROM [dbo].[Events] as SimoonaEvents
                Inner Join [dbo].[Walls] as walls
                On SimoonaEvents.Id = walls.Eventid
                Where walls.Eventid is not null");

            //Link posts to walls
            Sql(@"UPDATE [dbo].[Posts]
                SET
                    [WallId] = walls.Id
                FROM [dbo].[Posts] as Posts
                Inner Join [dbo].[Walls] as walls
                On Posts.EventId = walls.Eventid
                Where Posts.EventId is not null");

            //Add event walls moderators
            Sql(@"INSERT INTO [dbo].[WallModerators]
                    ([WallId]
                    ,[UserId]
                    ,[Created]
                    ,[CreatedBy]
                    ,[Modified]
                    ,[ModifiedBy])
                SELECT DISTINCT
                    walls.Id,
		            SimoonaEvents.ResponsibleUserId,
		            SimoonaEvents.Created,
		            SimoonaEvents.CreatedBy,
		            SimoonaEvents.Modified,
		            SimoonaEvents.ModifiedBy
                FROM[dbo].[Walls] as walls
                Inner Join[dbo].[Events] as SimoonaEvents
                On walls.id = SimoonaEvents.WallId");

            Sql(@"
                declare @userId varchar(128) = (SELECT Id FROM dbo.AspNetUsers WHERE IsManagingDirector = 1)

                INSERT INTO [dbo].[WallModerators]
                    ([WallId]
                    ,[UserId]
                    ,[Created]
                    ,[CreatedBy]
                    ,[Modified])
                SELECT
                    Walls.Id,
                    @userId,
                    Walls.Created,
	                @userId,
	                Walls.Created
                FROM [dbo].[Walls] as Walls
                LEFT JOIN [dbo].[WallModerators] AS Moderators ON Moderators.WallId = Walls.Id
                WHERE Moderators.UserId IS NULL");

            //Add event resposnisble persons to wall members
            Sql(@"INSERT INTO [dbo].[WallMembers]
                    ([WallId]
                    ,[UserId]
                    ,[Created]
                    ,[CreatedBy]
                    ,[Modified]
                    ,[ModifiedBy])
                SELECT DISTINCT
                    walls.Id,
		            SimoonaEvents.ResponsibleUserId,
		            SimoonaEvents.Created,
		            SimoonaEvents.CreatedBy,
		            SimoonaEvents.Modified,
		            SimoonaEvents.ModifiedBy
                FROM[dbo].[Walls] as walls
                Inner Join[dbo].[Events] as SimoonaEvents
                On walls.id = SimoonaEvents.WallId
                Inner Join[dbo].[WallMembers] as wallMembers
				On walls.id = wallMembers.WallId
				Where wallMembers.UserId != SimoonaEvents.ResponsibleUserId");

            //Add event participants to wall members
            Sql(@"INSERT INTO [dbo].[WallMembers]
                    ([WallId]
                    ,[UserId]
                    ,[Created]
                    ,[CreatedBy]
                    ,[Modified]
                    ,[ModifiedBy]
                    ,[IsDeleted])
                Select DISTINCT
                    walls.id,
	                participants.ApplicationUserId,
	                participants.Created,
	                participants.CreatedBy,
	                participants.Modified,
	                participants.ModifiedBy,
	                participants.IsDeleted
                From[dbo].[walls] as walls
                Inner join[dbo].[Events] as SimoonaEvents
                On walls.Id = SimoonaEvents.WallId
                Inner join[dbo].[EventParticipants] as participants
                On SimoonaEvents.Id = participants.EventId");

            DropColumn("dbo.Walls", "Eventid");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Events", "WallId", "dbo.Walls");
            DropIndex("dbo.Events", new[] { "WallId" });
            DropColumn("dbo.Events", "WallId");
        }
    }
}