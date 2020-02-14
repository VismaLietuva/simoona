namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUnnusedConnectionsWithBookLogs : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BookLogs", "BookId", "dbo.Books");
            DropForeignKey("dbo.BookLogs", "OfficeId", "dbo.Offices");
            DropIndex("dbo.BookLogs", new[] { "ApplicationUserId" });
            DropIndex("dbo.BookLogs", new[] { "BookId" });
            DropIndex("dbo.BookLogs", new[] { "OfficeId" });
            DropIndex("dbo.BookLogs", new[] { "OrganizationId" });
            DropIndex("dbo.Books", new[] { "OrganizationId" });
            AddColumn("dbo.Books", "Url", c => c.String(maxLength: 2000));
            AlterColumn("dbo.BookLogs", "ApplicationUserId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.BookLogs", "TakenFrom", c => c.DateTime(nullable: false));
            AlterColumn("dbo.BookLogs", "TakenTill", c => c.DateTime(nullable: false));
            AlterColumn("dbo.BookLogs", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.BookLogs", "CreatedBy", c => c.String(maxLength: 50));
            AlterColumn("dbo.BookLogs", "ModifiedBy", c => c.String(maxLength: 50));
            AlterColumn("dbo.Books", "Title", c => c.String(nullable: false));
            AlterColumn("dbo.Books", "Author", c => c.String(nullable: false));
            AlterColumn("dbo.Books", "Code", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.Books", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.Books", "CreatedBy", c => c.String(maxLength: 50));
            AlterColumn("dbo.Books", "ModifiedBy", c => c.String(maxLength: 50));
            AlterColumn("dbo.BookOffices", "CreatedBy", c => c.String(maxLength: 50));
            AlterColumn("dbo.BookOffices", "ModifiedBy", c => c.String(maxLength: 50));
            CreateIndex("dbo.BookLogs", "ApplicationUserId");
            CreateIndex("dbo.BookLogs", "BookOfficeId");
            CreateIndex("dbo.BookLogs", "OrganizationId");
            CreateIndex("dbo.Books", "OrganizationId");
            AddForeignKey("dbo.BookLogs", "BookOfficeId", "dbo.BookOffices", "Id");
            DropColumn("dbo.BookLogs", "BookId");
            DropColumn("dbo.BookLogs", "OfficeId");
            DropColumn("dbo.Books", "Quantity");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Books", "Quantity", c => c.Int(nullable: false));
            AddColumn("dbo.BookLogs", "OfficeId", c => c.Int(nullable: false));
            AddColumn("dbo.BookLogs", "BookId", c => c.Int(nullable: false));
            DropForeignKey("dbo.BookLogs", "BookOfficeId", "dbo.BookOffices");
            DropIndex("dbo.Books", new[] { "OrganizationId" });
            DropIndex("dbo.BookLogs", new[] { "OrganizationId" });
            DropIndex("dbo.BookLogs", new[] { "BookOfficeId" });
            DropIndex("dbo.BookLogs", new[] { "ApplicationUserId" });
            AlterColumn("dbo.BookOffices", "ModifiedBy", c => c.String());
            AlterColumn("dbo.BookOffices", "CreatedBy", c => c.String());
            AlterColumn("dbo.Books", "ModifiedBy", c => c.String());
            AlterColumn("dbo.Books", "CreatedBy", c => c.String());
            AlterColumn("dbo.Books", "OrganizationId", c => c.Int());
            AlterColumn("dbo.Books", "Code", c => c.String());
            AlterColumn("dbo.Books", "Author", c => c.String());
            AlterColumn("dbo.Books", "Title", c => c.String());
            AlterColumn("dbo.BookLogs", "ModifiedBy", c => c.String());
            AlterColumn("dbo.BookLogs", "CreatedBy", c => c.String());
            AlterColumn("dbo.BookLogs", "OrganizationId", c => c.Int());
            AlterColumn("dbo.BookLogs", "TakenTill", c => c.DateTime());
            AlterColumn("dbo.BookLogs", "TakenFrom", c => c.DateTime());
            AlterColumn("dbo.BookLogs", "ApplicationUserId", c => c.String(maxLength: 128));
            DropColumn("dbo.Books", "Url");
            CreateIndex("dbo.Books", "OrganizationId");
            CreateIndex("dbo.BookLogs", "OrganizationId");
            CreateIndex("dbo.BookLogs", "OfficeId");
            CreateIndex("dbo.BookLogs", "BookId");
            CreateIndex("dbo.BookLogs", "ApplicationUserId");
            AddForeignKey("dbo.BookLogs", "OfficeId", "dbo.Offices", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookLogs", "BookId", "dbo.Books", "Id", cascadeDelete: true);
        }
    }
}
