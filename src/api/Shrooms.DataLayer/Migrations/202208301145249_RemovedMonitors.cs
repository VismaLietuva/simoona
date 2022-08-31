namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedMonitors : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Monitors", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.Monitors", new[] { "OrganizationId" });
            DropTable("dbo.Monitors");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Monitors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Monitors", "OrganizationId");
            AddForeignKey("dbo.Monitors", "OrganizationId", "dbo.Organizations", "Id");
        }
    }
}
