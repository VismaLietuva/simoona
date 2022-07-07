namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFilterPresetEntityModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FilterPresets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsDefault = c.Boolean(nullable: false),
                        ForPage = c.Int(nullable: false),
                        Preset = c.String(nullable: false),
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
            DropForeignKey("dbo.FilterPresets", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.FilterPresets", new[] { "OrganizationId" });
            DropTable("dbo.FilterPresets");
        }
    }
}
