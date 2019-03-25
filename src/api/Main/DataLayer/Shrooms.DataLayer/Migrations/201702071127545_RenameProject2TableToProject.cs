namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameProject2TableToProject : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Project2ApplicationUser", newName: "ProjectApplicationUsers");
            RenameTable(name: "dbo.Project2Skill", newName: "ProjectSkills");
            RenameColumn(table: "dbo.ProjectApplicationUsers", name: "Project2_Id", newName: "Project_Id");
            RenameColumn(table: "dbo.ProjectSkills", name: "Project2_Id", newName: "Project_Id");
            RenameIndex(table: "dbo.ProjectApplicationUsers", name: "IX_Project2_Id", newName: "IX_Project_Id");
            RenameIndex(table: "dbo.ProjectSkills", name: "IX_Project2_Id", newName: "IX_Project_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ProjectSkills", name: "IX_Project_Id", newName: "IX_Project2_Id");
            RenameIndex(table: "dbo.ProjectApplicationUsers", name: "IX_Project_Id", newName: "IX_Project2_Id");
            RenameColumn(table: "dbo.ProjectSkills", name: "Project_Id", newName: "Project2_Id");
            RenameColumn(table: "dbo.ProjectApplicationUsers", name: "Project_Id", newName: "Project2_Id");
            RenameTable(name: "dbo.ProjectSkills", newName: "Project2Skill");
            RenameTable(name: "dbo.ProjectApplicationUsers", newName: "Project2ApplicationUser");
        }
    }
}
