namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMaxVirtualParticipantsFieldToEvent : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId)
                .Index(t => t.OrganizationId);
            
            AddColumn("dbo.Events", "MaxVirtualParticipants", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Monitors", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.Monitors", new[] { "OrganizationId" });
            DropColumn("dbo.Events", "MaxVirtualParticipants");
            DropTable("dbo.Monitors");
        }
    }
}
