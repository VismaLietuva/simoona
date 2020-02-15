namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedOrganizationFromComment : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Comments", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.Comments", new[] { "OrganizationId" });
            DropColumn("dbo.Comments", "OrganizationId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Comments", "OrganizationId", c => c.Int());
            CreateIndex("dbo.Comments", "OrganizationId");
            AddForeignKey("dbo.Comments", "OrganizationId", "dbo.Organizations", "Id");
        }
    }
}
