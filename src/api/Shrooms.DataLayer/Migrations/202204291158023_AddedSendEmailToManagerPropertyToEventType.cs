namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSendEmailToManagerPropertyToEventType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventTypes", "SendEmailToManager", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventTypes", "SendEmailToManager");
        }
    }
}
