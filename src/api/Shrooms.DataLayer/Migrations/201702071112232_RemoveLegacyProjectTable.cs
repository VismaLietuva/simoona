namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveLegacyProjectTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UsersProjects", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UsersProjects", "ProjectId", "dbo.AbstractClassifiers");
            DropIndex("dbo.UsersProjects", new[] { "UserId" });
            DropIndex("dbo.UsersProjects", new[] { "ProjectId" });
            DropTable("dbo.UsersProjects");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UsersProjects",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        ProjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.ProjectId });
            
            CreateIndex("dbo.UsersProjects", "ProjectId");
            CreateIndex("dbo.UsersProjects", "UserId");
            AddForeignKey("dbo.UsersProjects", "ProjectId", "dbo.AbstractClassifiers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UsersProjects", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
