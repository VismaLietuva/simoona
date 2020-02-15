namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveKnowYourColleagueGames : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.KnowYourColleagueLogs", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.KnowYourColleagueLogs", "PlayerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.KnowYourColleagueFakeUsers", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.KnowYourSpeakerLogs", "ExternalPlayerId", "dbo.KnowYourSpeakerExternalPlayers");
            DropIndex("dbo.KnowYourColleagueLogs", new[] { "PlayerId" });
            DropIndex("dbo.KnowYourColleagueLogs", new[] { "OrganizationId" });
            DropIndex("dbo.KnowYourColleagueFakeUsers", new[] { "OrganizationId" });
            DropIndex("dbo.KnowYourSpeakerLogs", new[] { "ExternalPlayerId" });
            DropTable("dbo.KnowYourColleagueLogs");
            DropTable("dbo.KnowYourColleagueFakeUsers");
            DropTable("dbo.KnowYourSpeakerExternalPlayers");
            DropTable("dbo.KnowYourSpeakerLogs");
            DropTable("dbo.Speakers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Speakers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        PhotoId = c.String(),
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
                        RawAnswers = c.String(),
                        RawQuestions = c.String(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.KnowYourColleagueFakeUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        PictureId = c.String(),
                        OrganizationId = c.Int(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.KnowYourColleagueLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Time = c.Time(nullable: false, precision: 7),
                        StartTime = c.DateTime(nullable: false),
                        CorrectAnswers = c.Int(nullable: false),
                        Difficulty = c.String(),
                        PlayerId = c.String(maxLength: 128),
                        OrganizationId = c.Int(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.KnowYourSpeakerLogs", "ExternalPlayerId");
            CreateIndex("dbo.KnowYourColleagueFakeUsers", "OrganizationId");
            CreateIndex("dbo.KnowYourColleagueLogs", "OrganizationId");
            CreateIndex("dbo.KnowYourColleagueLogs", "PlayerId");
            AddForeignKey("dbo.KnowYourSpeakerLogs", "ExternalPlayerId", "dbo.KnowYourSpeakerExternalPlayers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.KnowYourColleagueFakeUsers", "OrganizationId", "dbo.Organizations", "Id");
            AddForeignKey("dbo.KnowYourColleagueLogs", "PlayerId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.KnowYourColleagueLogs", "OrganizationId", "dbo.Organizations", "Id");
        }
    }
}
