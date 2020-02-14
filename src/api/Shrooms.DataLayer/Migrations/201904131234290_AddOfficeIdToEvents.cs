namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOfficeIdToEvents : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "OfficeId", c => c.Int());
            CreateIndex("dbo.Events", "OfficeId");
            AddForeignKey("dbo.Events", "OfficeId", "dbo.Offices", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Events", "OfficeId", "dbo.Offices");
            DropIndex("dbo.Events", new[] { "OfficeId" });
            DropColumn("dbo.Events", "OfficeId");
        }
    }
}
