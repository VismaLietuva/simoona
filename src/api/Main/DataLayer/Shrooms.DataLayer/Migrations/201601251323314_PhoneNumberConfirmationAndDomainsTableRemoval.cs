namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PhoneNumberConfirmationAndDomainsTableRemoval : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Domains", "Organization_Id", "dbo.Organizations");
            DropIndex("dbo.Domains", new[] { "Organization_Id" });
            AddColumn("dbo.Organizations", "HostName", c => c.String(maxLength: 50));
            AddColumn("dbo.Organizations", "HasRestrictedAccess", c => c.Boolean(nullable: false));
            DropColumn("dbo.AspNetUsers", "ShowPhoneNumber");
            DropTable("dbo.Domains");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Domains",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        UserDomain = c.String(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        Organization_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Name);
            
            AddColumn("dbo.AspNetUsers", "ShowPhoneNumber", c => c.Boolean(nullable: false));
            DropColumn("dbo.Organizations", "HasRestrictedAccess");
            DropColumn("dbo.Organizations", "HostName");
            CreateIndex("dbo.Domains", "Organization_Id");
            AddForeignKey("dbo.Domains", "Organization_Id", "dbo.Organizations", "Id", cascadeDelete: true);
        }
    }
}
