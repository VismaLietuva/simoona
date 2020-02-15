namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_CodeMonkeys : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CodeMonkeys", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.CodeMonkeysPresenters", "CodeMonkey_Id", "dbo.CodeMonkeys");
            DropForeignKey("dbo.CodeMonkeysPresenters", "AspNetUsers_Id", "dbo.AspNetUsers");
            DropIndex("dbo.CodeMonkeys", new[] { "OrganizationId" });
            DropIndex("dbo.CodeMonkeysPresenters", new[] { "CodeMonkey_Id" });
            DropIndex("dbo.CodeMonkeysPresenters", new[] { "AspNetUsers_Id" });
            DropTable("dbo.CodeMonkeys");
            DropTable("dbo.CodeMonkeysPresenters");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.CodeMonkeysPresenters",
                c => new
                    {
                        CodeMonkey_Id = c.Int(nullable: false),
                        AspNetUsers_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.CodeMonkey_Id, t.AspNetUsers_Id });
            
            CreateTable(
                "dbo.CodeMonkeys",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Topic = c.String(),
                        Date = c.DateTime(nullable: false),
                        Time = c.Time(nullable: false, precision: 7),
                        Location = c.String(),
                        SendEveryoneAnEmail = c.Boolean(nullable: false),
                        Description = c.String(),
                        GoogleCalendarId = c.String(),
                        GoogleCalendarLink = c.String(),
                        Kudosified = c.Boolean(nullable: false),
                        OrganizationId = c.Int(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.CodeMonkeysPresenters", "AspNetUsers_Id");
            CreateIndex("dbo.CodeMonkeysPresenters", "CodeMonkey_Id");
            CreateIndex("dbo.CodeMonkeys", "OrganizationId");
            AddForeignKey("dbo.CodeMonkeysPresenters", "AspNetUsers_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.CodeMonkeysPresenters", "CodeMonkey_Id", "dbo.CodeMonkeys", "Id", cascadeDelete: true);
            AddForeignKey("dbo.CodeMonkeys", "OrganizationId", "dbo.Organizations", "Id");
        }
    }
}
