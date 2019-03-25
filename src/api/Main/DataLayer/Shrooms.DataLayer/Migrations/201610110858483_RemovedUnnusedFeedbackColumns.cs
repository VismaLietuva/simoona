namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedUnnusedFeedbackColumns : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Feedbacks", "TimestampSent");
            DropColumn("dbo.Feedbacks", "EmailSentTime");
            DropColumn("dbo.Feedbacks", "SentEmailAddresses");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Feedbacks", "SentEmailAddresses", c => c.String());
            AddColumn("dbo.Feedbacks", "EmailSentTime", c => c.DateTime());
            AddColumn("dbo.Feedbacks", "TimestampSent", c => c.DateTime(nullable: false));
        }
    }
}
