namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveVacationModuleTables : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Deputies", "deputy_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Deputies", "employee_id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Vacation_Period", "employee_id", "dbo.AspNetUsers");
            DropIndex("dbo.Deputies", new[] { "employee_id" });
            DropIndex("dbo.Deputies", new[] { "deputy_id" });
            DropIndex("dbo.Vacation_Period", new[] { "employee_id" });
            DropTable("dbo.Deputies");
            DropTable("dbo.Vacation_Period");
            DropTable("dbo.VacationTypes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.VacationTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        AnnualLeave = c.Boolean(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Vacation_Period",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        employee_id = c.String(maxLength: 128),
                        from_date = c.DateTime(nullable: false),
                        type = c.Int(nullable: false),
                        to_date = c.DateTime(nullable: false),
                        ordered_date = c.DateTime(),
                        approved_date = c.DateTime(),
                        channel = c.String(),
                        status = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Deputies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        employee_id = c.String(maxLength: 128),
                        deputy_id = c.String(maxLength: 128),
                        from_date = c.DateTime(nullable: false),
                        to_date = c.DateTime(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Vacation_Period", "employee_id");
            CreateIndex("dbo.Deputies", "deputy_id");
            CreateIndex("dbo.Deputies", "employee_id");
            AddForeignKey("dbo.Vacation_Period", "employee_id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Deputies", "employee_id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Deputies", "deputy_id", "dbo.AspNetUsers", "Id");
        }
    }
}
