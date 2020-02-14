namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddEventTypesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventTypes",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    IsSingleJoin = c.Boolean(nullable: false),
                    OrganizationId = c.Int(nullable: false),
                    Created = c.DateTime(nullable: false),
                    CreatedBy = c.String(maxLength: 50),
                    Modified = c.DateTime(nullable: false),
                    ModifiedBy = c.String(maxLength: 50),
                    IsDeleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId)
                .Index(t => t.OrganizationId);

            AddColumn("dbo.Events", "EventTypeId", c => c.Int());
            CreateIndex("dbo.Events", "EventTypeId");
            AddForeignKey("dbo.Events", "EventTypeId", "dbo.EventTypes", "Id");

            Sql(@"DECLARE
					@foodEventName varchar(60),
					@sportsEventName varchar(60),
					@leisureEventName varchar(60),
					@organizationId int,
					@foodEventId int,
					@sportsEventId int,
					@leisureEventId int

				SELECT
					@foodEventName = 'foodEventType',
					@sportsEventName = 'sportsEventType',
					@leisureEventName = 'leisureEventType',
					@organizationId = 2

				INSERT INTO EventTypes
					(Name, Created, IsDeleted, IsSingleJoin, Modified, OrganizationId)
					VALUES
					(@foodEventName, GETDATE(), 0, 1, GETDATE(), @organizationId)

				INSERT INTO EventTypes
					(Name, Created, IsDeleted, IsSingleJoin, Modified, OrganizationId)
					VALUES
					(@sportsEventName, GETDATE(), 0, 0, GETDATE(), @organizationId)

				INSERT INTO EventTypes
					(Name, Created, IsDeleted, IsSingleJoin, Modified, OrganizationId)
					VALUES
					(@leisureEventName, GETDATE(), 0, 0, GETDATE(), @organizationId)

				SELECT
					@foodEventId = (SELECT Id FROM EventTypes WHERE Name = @foodEventName),
					@sportsEventId = (SELECT Id FROM EventTypes WHERE Name = @sportsEventName),
					@leisureEventId = (SELECT Id FROM EventTypes WHERE Name = @leisureEventName)

				UPDATE Events
				SET EventTypeId = @foodEventId
				WHERE IsFoodEvent = 1 OR IsFoodEvent = 0 AND Name = 'la crepe'

				UPDATE Events
				SET EventTypeId = @sportsEventId
				WHERE IsFoodEvent = 0 AND Name NOT LIKE '%board%' AND Name NOT LIKE 'la crepe'

				UPDATE Events
				SET EventTypeId = @leisureEventId
				WHERE IsFoodEvent = 0 AND Name LIKE '%board%'");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Events", "EventTypeId", "dbo.EventTypes");
            DropForeignKey("dbo.EventTypes", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.EventTypes", new[] { "OrganizationId" });
            DropIndex("dbo.Events", new[] { "EventTypeId" });
            DropColumn("dbo.Events", "EventTypeId");
            DropTable("dbo.EventTypes");
        }
    }
}
