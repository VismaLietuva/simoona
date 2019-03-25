namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalLinksTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExternalLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Url = c.String(nullable: false),
                        OrganizationId = c.Int(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId)
                .Index(t => t.OrganizationId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExternalLinks", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.ExternalLinks", new[] { "OrganizationId" });
            DropTable("dbo.ExternalLinks");
        }
    }
}
