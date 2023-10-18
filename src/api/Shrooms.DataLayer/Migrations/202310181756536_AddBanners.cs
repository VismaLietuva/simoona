namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBanners : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Banners",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Url = c.String(),
                        PictureId = c.String(nullable: false),
                        ValidFrom = c.DateTime(precision: 7, storeType: "datetime2"),
                        ValidTo = c.DateTime(precision: 7, storeType: "datetime2"),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Banners", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.Banners", new[] { "OrganizationId" });
            DropTable("dbo.Banners");
        }
    }
}
