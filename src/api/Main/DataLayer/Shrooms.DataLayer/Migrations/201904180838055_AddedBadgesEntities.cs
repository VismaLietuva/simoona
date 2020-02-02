namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBadgesEntities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BadgeCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BadgeTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        ImageUrl = c.String(),
                        ImageSmallUrl = c.String(),
                        Value = c.Int(nullable: false),
                        BadgeCategoryId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BadgeCategories", t => t.BadgeCategoryId, cascadeDelete: true)
                .Index(t => t.BadgeCategoryId);
            
            CreateTable(
                "dbo.BadgeCategoryKudosTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CalculationPolicyType = c.Int(nullable: false),
                        BadgeCategoryId = c.Int(nullable: false),
                        KudosTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BadgeCategories", t => t.BadgeCategoryId, cascadeDelete: true)
                .ForeignKey("dbo.KudosTypes", t => t.KudosTypeId, cascadeDelete: true)
                .Index(t => t.BadgeCategoryId)
                .Index(t => t.KudosTypeId);
            
            CreateTable(
                "dbo.BadgeLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmployeeId = c.String(maxLength: 128),
                        BadgeTypeId = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BadgeTypes", t => t.BadgeTypeId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.EmployeeId)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.EmployeeId)
                .Index(t => t.BadgeTypeId)
                .Index(t => t.OrganizationId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.BadgeLogs", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.BadgeLogs", "EmployeeId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BadgeLogs", "BadgeTypeId", "dbo.BadgeTypes");
            DropForeignKey("dbo.BadgeCategoryKudosTypes", "KudosTypeId", "dbo.KudosTypes");
            DropForeignKey("dbo.BadgeCategoryKudosTypes", "BadgeCategoryId", "dbo.BadgeCategories");
            DropForeignKey("dbo.BadgeTypes", "BadgeCategoryId", "dbo.BadgeCategories");
            DropIndex("dbo.BadgeLogs", new[] { "OrganizationId" });
            DropIndex("dbo.BadgeLogs", new[] { "BadgeTypeId" });
            DropIndex("dbo.BadgeLogs", new[] { "EmployeeId" });
            DropIndex("dbo.BadgeCategoryKudosTypes", new[] { "KudosTypeId" });
            DropIndex("dbo.BadgeCategoryKudosTypes", new[] { "BadgeCategoryId" });
            DropIndex("dbo.BadgeTypes", new[] { "BadgeCategoryId" });
            DropTable("dbo.BadgeLogs");
            DropTable("dbo.BadgeCategoryKudosTypes");
            DropTable("dbo.BadgeTypes");
            DropTable("dbo.BadgeCategories");
        }
    }
}
