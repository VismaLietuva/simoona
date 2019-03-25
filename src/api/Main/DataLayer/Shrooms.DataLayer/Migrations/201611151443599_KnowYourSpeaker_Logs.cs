namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class KnowYourSpeaker_Logs : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KnowYourSpeakerLogs", "RawAnswers", c => c.String());
            AddColumn("dbo.KnowYourSpeakerLogs", "RawQuestions", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.KnowYourSpeakerLogs", "RawAnswers");
            DropColumn("dbo.KnowYourSpeakerLogs", "RawQuestions");
        }
    }
}
