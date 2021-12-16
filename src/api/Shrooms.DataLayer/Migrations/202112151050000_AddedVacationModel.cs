namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedVacationModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VacationPages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VacationPages", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.VacationPages", new[] { "OrganizationId" });
            DropTable("dbo.VacationPages");
        }
    }
}
