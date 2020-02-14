namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddKnowYourSpeakerLogAndPlayer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KnowYourSpeakerExternalPlayers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.KnowYourSpeakerLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Time = c.Time(nullable: false, precision: 7),
                        StartTime = c.DateTime(nullable: false),
                        CorrectAnswers = c.Int(nullable: false),
                        ExternalPlayerId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KnowYourSpeakerExternalPlayers", t => t.ExternalPlayerId, cascadeDelete: true)
                .Index(t => t.ExternalPlayerId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.KnowYourSpeakerLogs", "ExternalPlayerId", "dbo.KnowYourSpeakerExternalPlayers");
            DropIndex("dbo.KnowYourSpeakerLogs", new[] { "ExternalPlayerId" });
            DropTable("dbo.KnowYourSpeakerLogs");
            DropTable("dbo.KnowYourSpeakerExternalPlayers");
        }
    }
}
