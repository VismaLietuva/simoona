namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveHashtags : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Hashtags", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.Hashtags", new[] { "OrganizationId" });
            DropTable("dbo.Hashtags");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Hashtags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        Mentions = c.Int(nullable: false),
                        Important = c.Boolean(nullable: false),
                        OrganizationId = c.Int(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Hashtags", "OrganizationId");
            AddForeignKey("dbo.Hashtags", "OrganizationId", "dbo.Organizations", "Id");
        }
    }
}
