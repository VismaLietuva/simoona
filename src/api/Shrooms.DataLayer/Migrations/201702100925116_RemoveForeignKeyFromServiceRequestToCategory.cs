namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveForeignKeyFromServiceRequestToCategory : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ServiceRequests", "ServiceRequestCategoryId", "dbo.ServiceRequestCategories");
            DropIndex("dbo.ServiceRequests", new[] { "ServiceRequestCategoryId" });
            DropColumn("dbo.ServiceRequests", "ServiceRequestCategoryId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ServiceRequests", "ServiceRequestCategoryId", c => c.Int(nullable: false));
            CreateIndex("dbo.ServiceRequests", "ServiceRequestCategoryId");
            AddForeignKey("dbo.ServiceRequests", "ServiceRequestCategoryId", "dbo.ServiceRequestCategories", "Id", cascadeDelete: true);
        }
    }
}
