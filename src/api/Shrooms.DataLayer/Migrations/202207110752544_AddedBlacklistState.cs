namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBlacklistState : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlacklistStates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Reason = c.String(),
                        UserId = c.String(nullable: false, maxLength: 128),
                        EndDate = c.DateTime(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.OrganizationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlacklistStates", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlacklistStates", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.BlacklistStates", new[] { "OrganizationId" });
            DropIndex("dbo.BlacklistStates", new[] { "UserId" });
            DropTable("dbo.BlacklistStates");
        }
    }
}
