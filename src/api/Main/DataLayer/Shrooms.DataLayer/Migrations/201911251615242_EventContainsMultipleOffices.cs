namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EventContainsMultipleOffices : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "Offices", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "Offices");
        }
    }
}
