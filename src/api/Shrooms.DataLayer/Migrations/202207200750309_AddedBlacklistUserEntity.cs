namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBlacklistUserEntity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlacklistUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Reason = c.String(),
                        UserId = c.String(nullable: false, maxLength: 128),
                        EndDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        ModifiedBy = c.String(nullable: false, maxLength: 128),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ModifiedBy)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ModifiedBy)
                .Index(t => t.OrganizationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlacklistUsers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlacklistUsers", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.BlacklistUsers", "ModifiedBy", "dbo.AspNetUsers");
            DropIndex("dbo.BlacklistUsers", new[] { "OrganizationId" });
            DropIndex("dbo.BlacklistUsers", new[] { "ModifiedBy" });
            DropIndex("dbo.BlacklistUsers", new[] { "UserId" });
            DropTable("dbo.BlacklistUsers");
        }
    }
}
