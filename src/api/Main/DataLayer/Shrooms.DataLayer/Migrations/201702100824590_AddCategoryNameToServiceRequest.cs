namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryNameToServiceRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceRequests", "CategoryName", c => c.String());
            Sql(@"UPDATE [dbo].[ServiceRequests]
                  SET [CategoryName] = requestCat.Name
                  FROM [dbo].[ServiceRequests] as request
                  INNER JOIN [dbo].[ServiceRequestCategories] as requestCat
                  ON request.ServiceRequestCategoryId = requestCat.Id");
        }
        
        public override void Down()
        {
            DropColumn("dbo.ServiceRequests", "CategoryName");
        }
    }
}
