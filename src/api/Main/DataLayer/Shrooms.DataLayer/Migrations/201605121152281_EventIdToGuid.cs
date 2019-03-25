namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EventIdToGuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "Id2", c => c.Guid(nullable: false, identity: true, defaultValueSql: "newid()"));
            AddColumn("dbo.EventOptions", "EventId2", c => c.Guid(nullable: false));
            AddColumn("dbo.EventParticipants", "EventId2", c => c.Guid(nullable: false));
            AddColumn("dbo.EventComments", "EventId2", c => c.Guid(nullable: false));
            Sql(@"UPDATE Comments
                SET Comments.EventId2 = Events.Id2
                FROM [dbo].[EventComments] AS Comments
                JOIN [dbo].[Events] AS Events ON Events.Id = Comments.EventId

                UPDATE Options
                SET Options.EventId2 = Events.Id2
                FROM [dbo].[EventOptions] AS Options
                JOIN [dbo].[Events] AS Events ON Events.Id = Options.EventId

                UPDATE Participants
                SET Participants.EventId2 = Events.Id2
                FROM [dbo].[EventParticipants] AS Participants
                JOIN [dbo].[Events] AS Events ON Events.Id = Participants.EventId");

            DropForeignKey("dbo.EventComments", "EventId", "dbo.Events");
            DropForeignKey("dbo.EventOptions", "EventId", "dbo.Events");
            DropForeignKey("dbo.EventParticipants", "EventId", "dbo.Events");
            DropIndex("dbo.EventComments", new[] { "EventId" });
            DropIndex("dbo.EventOptions", new[] { "EventId" });
            DropIndex("dbo.EventParticipants", new[] { "EventId" });
            DropColumn("dbo.EventComments", "EventId");
            DropColumn("dbo.EventOptions", "EventId");
            DropColumn("dbo.EventParticipants", "EventId");
            DropPrimaryKey("dbo.Events");
            DropColumn("dbo.Events", "Id");
            RenameColumn(table: "dbo.EventParticipants", name: "EventId2", newName: "EventId");
            RenameColumn(table: "dbo.EventOptions", name: "EventId2", newName: "EventId");
            RenameColumn(table: "dbo.EventComments", name: "EventId2", newName: "EventId");
            RenameColumn(table: "dbo.Events", name: "Id2", newName: "Id");
            AddPrimaryKey("dbo.Events", "Id");
            CreateIndex("dbo.EventComments", "EventId");
            CreateIndex("dbo.EventOptions", "EventId");
            CreateIndex("dbo.EventParticipants", "EventId");
            AddForeignKey("dbo.EventComments", "EventId", "dbo.Events", "Id", cascadeDelete: false);
            AddForeignKey("dbo.EventOptions", "EventId", "dbo.Events", "Id", cascadeDelete: false);
            AddForeignKey("dbo.EventParticipants", "EventId", "dbo.Events", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
        }
    }
}
