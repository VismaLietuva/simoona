namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveEventParticipantOptionTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EventParticipantOptions", "EventParticipantId", "dbo.EventParticipants");
            DropIndex("dbo.EventParticipantOptions", new[] { "EventParticipantId" });
            DropTable("dbo.EventParticipantOptions");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.EventParticipantOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Option = c.String(nullable: false),
                        EventParticipantId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.EventParticipantOptions", "EventParticipantId");
            AddForeignKey("dbo.EventParticipantOptions", "EventParticipantId", "dbo.EventParticipants", "Id", cascadeDelete: true);
        }
    }
}
