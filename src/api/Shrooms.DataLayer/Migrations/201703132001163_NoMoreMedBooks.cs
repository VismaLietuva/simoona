namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NoMoreMedBooks : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MedicalBooksOptions", "Id", "dbo.Organizations");
            DropIndex("dbo.MedicalBooksOptions", new[] { "Id" });
            DropColumn("dbo.AspNetUsers", "MedicalBookChecked");
            DropColumn("dbo.AspNetUsers", "MedicalBookValidTill");
            DropColumn("dbo.Organizations", "MedicalBooksOptionId");
            DropTable("dbo.MedicalBooksOptions");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MedicalBooksOptions",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Expiration = c.Int(nullable: false),
                        Frequency = c.String(),
                        WeekDay = c.String(),
                        NextFireTime = c.DateTimeOffset(precision: 7),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Organizations", "MedicalBooksOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "MedicalBookValidTill", c => c.DateTime());
            AddColumn("dbo.AspNetUsers", "MedicalBookChecked", c => c.DateTime());
            CreateIndex("dbo.MedicalBooksOptions", "Id");
            AddForeignKey("dbo.MedicalBooksOptions", "Id", "dbo.Organizations", "Id");
        }
    }
}
