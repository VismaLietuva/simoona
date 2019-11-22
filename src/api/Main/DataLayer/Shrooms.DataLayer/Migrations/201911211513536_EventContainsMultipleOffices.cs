namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EventContainsMultipleOffices : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Events", "OfficeId", "dbo.Offices");
            DropIndex("dbo.Events", new[] { "OfficeId" });
            AddColumn("dbo.Events", "Offices", c => c.String());
            DropColumn("dbo.Events", "OfficeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Events", "OfficeId", c => c.Int());
            DropColumn("dbo.Events", "Offices");
            CreateIndex("dbo.Events", "OfficeId");
            AddForeignKey("dbo.Events", "OfficeId", "dbo.Offices", "Id");
        }
    }
}
