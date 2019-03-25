namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveEventTimezoneColumn : DbMigration
    {
        public override void Up()
        {
            // DropColumn("dbo.Events", "TimeZone");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Events", "TimeZone", c => c.String());
        }
    }
}
