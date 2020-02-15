namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddRestrictionsToEventTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EventComments", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Events", "ResponsibleUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.EventParticipants", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.EventParticipantOptions", "EventParticipantId", "dbo.EventParticipants");
            DropIndex("dbo.Events", new[] { "EventTypeId" });
            DropIndex("dbo.Events", new[] { "OrganizationId" });
            DropIndex("dbo.Events", new[] { "ResponsibleUser_Id" });
            RenameColumn(table: "dbo.EventComments", name: "ApplicationUser_Id", newName: "ApplicationUserId");
            RenameColumn(table: "dbo.Events", name: "ResponsibleUser_Id", newName: "ResponsibleUserId");
            RenameColumn(table: "dbo.EventParticipants", name: "ApplicationUser_Id", newName: "ApplicationUserId");
            RenameColumn(table: "dbo.Events", name: "Date", newName: "StartDate");
            RenameIndex(table: "dbo.EventComments", name: "IX_ApplicationUser_Id", newName: "IX_ApplicationUserId");
            RenameIndex(table: "dbo.EventParticipants", name: "IX_ApplicationUser_Id", newName: "IX_ApplicationUserId");
            CreateTable(
                "dbo.EventParticipantEventOptions",
                c => new
                {
                    EventParticipant_Id = c.Int(nullable: false),
                    EventOption_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.EventParticipant_Id, t.EventOption_Id })
                .ForeignKey("dbo.EventParticipants", t => t.EventParticipant_Id, cascadeDelete: true)
                .ForeignKey("dbo.EventOptions", t => t.EventOption_Id, cascadeDelete: true)
                .Index(t => t.EventParticipant_Id)
                .Index(t => t.EventOption_Id);

            AddColumn("dbo.Events", "EndDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Events", "StartDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.EventComments", "Comment", c => c.String(nullable: false));
            AlterColumn("dbo.Events", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Events", "Place", c => c.String(nullable: false));
            AlterColumn("dbo.Events", "EventTypeId", c => c.Int(nullable: false));
            AlterColumn("dbo.Events", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.Events", "ResponsibleUserId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.EventTypes", "CreatedBy", c => c.String());
            AlterColumn("dbo.EventTypes", "ModifiedBy", c => c.String());
            CreateIndex("dbo.Events", "StartDate", name: "ix_start_date");
            CreateIndex("dbo.Events", "EndDate", name: "ix_end_date");
            CreateIndex("dbo.Events", "EventTypeId");
            CreateIndex("dbo.Events", "ResponsibleUserId");
            CreateIndex("dbo.Events", "OrganizationId");
            CreateIndex("dbo.EventParticipants", "Created", name: "ix_created_date");
            AddForeignKey("dbo.Events", "ResponsibleUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.EventComments", "ApplicationUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.EventParticipants", "ApplicationUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.EventParticipantOptions", "EventParticipantId", "dbo.EventParticipants", "Id", cascadeDelete: true);
            DropColumn("dbo.Events", "IsOptions");
            DropColumn("dbo.Events", "IsFoodEvent");
            DropColumn("dbo.Events", "IsPizzaEvent");
            DropColumn("dbo.Events", "TemplateText");
            DropColumn("dbo.Events", "TemplateEmail");

            Sql(@"INSERT INTO [dbo].[EventParticipantEventOptions]
					([EventParticipant_Id]
					,[EventOption_Id])
				
				SELECT 
					ParticipantOption.EventParticipantId,
					EventOption.Id
				FROM [dbo].[EventParticipantOptions] as ParticipantOption
				JOIN [dbo].[EventOptions] as EventOption
				On ParticipantOption.[Option] = EventOption.[Option]

				Where ParticipantOption.IsDeleted = 0 AND EventOption.IsDeleted = 0");

            Sql(@"UPDATE [dbo].[Events] SET EndDate = DATEADD(hour, 2, StartDate)");
        }

        public override void Down()
        {
            AddColumn("dbo.Events", "TemplateEmail", c => c.String());
            AddColumn("dbo.Events", "TemplateText", c => c.String());
            AddColumn("dbo.Events", "IsPizzaEvent", c => c.Boolean(nullable: false));
            AddColumn("dbo.Events", "IsFoodEvent", c => c.Boolean(nullable: false));
            AddColumn("dbo.Events", "IsOptions", c => c.Boolean(nullable: false));
            AddColumn("dbo.Events", "Date", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.EventParticipantOptions", "EventParticipantId", "dbo.EventParticipants");
            DropForeignKey("dbo.EventParticipants", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EventComments", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Events", "ResponsibleUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EventParticipantEventOptions", "EventOption_Id", "dbo.EventOptions");
            DropForeignKey("dbo.EventParticipantEventOptions", "EventParticipant_Id", "dbo.EventParticipants");
            DropIndex("dbo.EventParticipantEventOptions", new[] { "EventOption_Id" });
            DropIndex("dbo.EventParticipantEventOptions", new[] { "EventParticipant_Id" });
            DropIndex("dbo.EventParticipants", "ix_created_date");
            DropIndex("dbo.Events", new[] { "OrganizationId" });
            DropIndex("dbo.Events", new[] { "ResponsibleUserId" });
            DropIndex("dbo.Events", new[] { "EventTypeId" });
            DropIndex("dbo.Events", "ix_end_date");
            DropIndex("dbo.Events", "ix_start_date");
            AlterColumn("dbo.EventTypes", "ModifiedBy", c => c.String(maxLength: 50));
            AlterColumn("dbo.EventTypes", "CreatedBy", c => c.String(maxLength: 50));
            AlterColumn("dbo.Events", "ResponsibleUserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Events", "OrganizationId", c => c.Int());
            AlterColumn("dbo.Events", "EventTypeId", c => c.Int());
            AlterColumn("dbo.Events", "Place", c => c.String(maxLength: 50));
            AlterColumn("dbo.Events", "Name", c => c.String(maxLength: 50));
            AlterColumn("dbo.EventComments", "Comment", c => c.String());
            DropColumn("dbo.Events", "EndDate");
            DropColumn("dbo.Events", "StartDate");
            DropTable("dbo.EventParticipantEventOptions");
            RenameIndex(table: "dbo.EventParticipants", name: "IX_ApplicationUserId", newName: "IX_ApplicationUser_Id");
            RenameIndex(table: "dbo.EventComments", name: "IX_ApplicationUserId", newName: "IX_ApplicationUser_Id");
            RenameColumn(table: "dbo.EventParticipants", name: "ApplicationUserId", newName: "ApplicationUser_Id");
            RenameColumn(table: "dbo.Events", name: "ResponsibleUserId", newName: "ResponsibleUser_Id");
            RenameColumn(table: "dbo.EventComments", name: "ApplicationUserId", newName: "ApplicationUser_Id");
            CreateIndex("dbo.Events", "ResponsibleUser_Id");
            CreateIndex("dbo.Events", "OrganizationId");
            CreateIndex("dbo.Events", "EventTypeId");
            AddForeignKey("dbo.EventParticipantOptions", "EventParticipantId", "dbo.EventParticipants", "Id");
            AddForeignKey("dbo.EventParticipants", "ApplicationUser_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "ResponsibleUser_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.EventComments", "ApplicationUser_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
