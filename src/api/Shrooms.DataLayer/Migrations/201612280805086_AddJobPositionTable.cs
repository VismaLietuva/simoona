namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddJobPositionTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobPositions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
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
            
            AddColumn("dbo.AspNetUsers", "JobPositionId", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "JobPositionId");
            AddForeignKey("dbo.AspNetUsers", "JobPositionId", "dbo.JobPositions", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "JobPositionId", "dbo.JobPositions");
            DropForeignKey("dbo.JobPositions", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.JobPositions", new[] { "OrganizationId" });
            DropIndex("dbo.AspNetUsers", new[] { "JobPositionId" });
            DropColumn("dbo.AspNetUsers", "JobPositionId");
            DropTable("dbo.JobPositions");
        }
    }
}
