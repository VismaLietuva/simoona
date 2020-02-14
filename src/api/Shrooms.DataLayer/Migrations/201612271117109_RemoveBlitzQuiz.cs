namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveBlitzQuiz : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BlitzQuizAnswers", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlitzQuizAnswerLink", "BlitzQuizs_Id", "dbo.BlitzQuizs");
            DropForeignKey("dbo.BlitzQuizAnswerLink", "BlitzQuizAnswers_Id", "dbo.BlitzQuizAnswers");
            DropForeignKey("dbo.BlitzQuizs", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlitzQuizs", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.BlitzQuizAnswers", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.BlitzQuizs", new[] { "OrganizationId" });
            DropIndex("dbo.BlitzQuizs", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.BlitzQuizAnswerLink", new[] { "BlitzQuizs_Id" });
            DropIndex("dbo.BlitzQuizAnswerLink", new[] { "BlitzQuizAnswers_Id" });
            DropTable("dbo.BlitzQuizAnswers");
            DropTable("dbo.BlitzQuizs");
            DropTable("dbo.BlitzQuizAnswerLink");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.BlitzQuizAnswerLink",
                c => new
                    {
                        BlitzQuizs_Id = c.Int(nullable: false),
                        BlitzQuizAnswers_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BlitzQuizs_Id, t.BlitzQuizAnswers_Id });
            
            CreateTable(
                "dbo.BlitzQuizs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Question = c.String(),
                        Category = c.String(),
                        Date = c.DateTime(nullable: false),
                        Time = c.Time(nullable: false, precision: 7),
                        OrganizationId = c.Int(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BlitzQuizAnswers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Answer = c.String(),
                        Correct = c.Boolean(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.BlitzQuizAnswerLink", "BlitzQuizAnswers_Id");
            CreateIndex("dbo.BlitzQuizAnswerLink", "BlitzQuizs_Id");
            CreateIndex("dbo.BlitzQuizs", "ApplicationUser_Id");
            CreateIndex("dbo.BlitzQuizs", "OrganizationId");
            CreateIndex("dbo.BlitzQuizAnswers", "ApplicationUser_Id");
            AddForeignKey("dbo.BlitzQuizs", "OrganizationId", "dbo.Organizations", "Id");
            AddForeignKey("dbo.BlitzQuizs", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.BlitzQuizAnswerLink", "BlitzQuizAnswers_Id", "dbo.BlitzQuizAnswers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BlitzQuizAnswerLink", "BlitzQuizs_Id", "dbo.BlitzQuizs", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BlitzQuizAnswers", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
