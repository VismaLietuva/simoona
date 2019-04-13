namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBadgesEntities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BadgeCalculators",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ClassName = c.String(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BadgeCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        BadgeCalculatorId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BadgeCalculators", t => t.BadgeCalculatorId, cascadeDelete: true)
                .Index(t => t.BadgeCalculatorId);
            
            CreateTable(
                "dbo.BadgeCategoryKudosTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BadgeCategoryId = c.Int(nullable: false),
                        KudosTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BadgeCategories", t => t.BadgeCategoryId, cascadeDelete: true)
                .ForeignKey("dbo.KudosTypes", t => t.KudosTypeId, cascadeDelete: true)
                .Index(t => t.BadgeCategoryId)
                .Index(t => t.KudosTypeId);
            
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BadgeTypes", "BadgeCategoryId", "dbo.BadgeCategories");
            DropForeignKey("dbo.BadgeCategoryKudosTypes", "KudosTypeId", "dbo.KudosTypes");
            DropForeignKey("dbo.BadgeCategoryKudosTypes", "BadgeCategoryId", "dbo.BadgeCategories");
            DropForeignKey("dbo.BadgeCategories", "BadgeCalculatorId", "dbo.BadgeCalculators");
            DropIndex("dbo.BadgeTypes", new[] { "BadgeCategoryId" });
            DropIndex("dbo.BadgeCategoryKudosTypes", new[] { "KudosTypeId" });
            DropIndex("dbo.BadgeCategoryKudosTypes", new[] { "BadgeCategoryId" });
            DropIndex("dbo.BadgeCategories", new[] { "BadgeCalculatorId" });
            DropTable("dbo.BadgeTypes");
            DropTable("dbo.BadgeCategoryKudosTypes");
            DropTable("dbo.BadgeCategories");
            DropTable("dbo.BadgeCalculators");
        }
    }
}
