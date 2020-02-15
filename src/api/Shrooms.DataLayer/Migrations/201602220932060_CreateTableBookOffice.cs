namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTableBookOffice : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BookOffices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BookId = c.Int(nullable: false),
                        OfficeId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Books", t => t.BookId, cascadeDelete: true)
                .ForeignKey("dbo.Offices", t => t.OfficeId)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId)
                .Index(t => new { t.BookId, t.OfficeId }, unique: true, name: "BookId_OfficeId")
                .Index(t => t.OrganizationId);
            
            AddColumn("dbo.BookLogs", "BookOfficeId", c => c.Int(nullable: false));

            Sql(
                "INSERT INTO BookOffices " +
                    "(Logs.BookId, " +
                    "Logs.OfficeId, " +
                    "Books.Quantity, " +
                    "Logs.OrganizationId, " +
                    "Books.Created, " +
                    "Books.CreatedBy, " +
                    "Books.Modified, " +
                    "Books.ModifiedBy, " +
                    "Books.IsDeleted) " +
                "SELECT DISTINCT " +
                    "Logs.BookId, " +
                    "Logs.OfficeId, " +
                    "Books.Quantity, " +
                    "Logs.OrganizationId, " +
                    "Books.Created, " +
                    "Books.CreatedBy, " +
                    "Books.Modified, " +
                    "Books.ModifiedBy, " +
                    "Books.IsDeleted " +
                "FROM BookLogs as Logs " +
                "INNER JOIN Books as Books " +
                "ON Logs.BookId = Books.Id " +
                "INNER JOIN Offices as Office " +
                "ON Logs.OfficeId = Office.Id");

            Sql(
                "UPDATE BookLogs " +
                    "SET " +
                    "BookLogs.BookOfficeId = BookOffices.Id " +
                    "FROM BookOffices " +
                    "INNER JOIN BookLogs " +
                    "ON BookOffices.BookId = BookLogs.BookId AND BookOffices.OfficeId = BookLogs.OfficeId");

            Sql("DELETE FROM[dbo].[BookLogs] " +
               "Where[ApplicationUserId] is null");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BookOffices", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.BookOffices", "OfficeId", "dbo.Offices");
            DropForeignKey("dbo.BookOffices", "BookId", "dbo.Books");
            DropIndex("dbo.BookOffices", new[] { "OrganizationId" });
            DropIndex("dbo.BookOffices", "BookId_OfficeId");
            DropColumn("dbo.BookLogs", "BookOfficeId");
            DropTable("dbo.BookOffices");
        }
    }
}
