namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCalendarForTenant : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "CalendarId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organizations", "CalendarId");
        }
    }
}
