namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSendEmailToCoachPropertyToEventType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventTypes", "SendEmailToCoach", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventTypes", "SendEmailToCoach");
        }
    }
}
