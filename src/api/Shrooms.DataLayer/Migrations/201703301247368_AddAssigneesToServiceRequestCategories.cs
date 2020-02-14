namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAssigneesToServiceRequestCategories : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ServiceRequestCategoryApplicationUsers",
                c => new
                    {
                        ServiceRequestCategory_Id = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ServiceRequestCategory_Id, t.ApplicationUser_Id })
                .ForeignKey("dbo.ServiceRequestCategories", t => t.ServiceRequestCategory_Id, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .Index(t => t.ServiceRequestCategory_Id)
                .Index(t => t.ApplicationUser_Id);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ServiceRequestCategoryApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceRequestCategoryApplicationUsers", "ServiceRequestCategory_Id", "dbo.ServiceRequestCategories");
            DropIndex("dbo.ServiceRequestCategoryApplicationUsers", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.ServiceRequestCategoryApplicationUsers", new[] { "ServiceRequestCategory_Id" });
            DropTable("dbo.ServiceRequestCategoryApplicationUsers");
        }
    }
}
