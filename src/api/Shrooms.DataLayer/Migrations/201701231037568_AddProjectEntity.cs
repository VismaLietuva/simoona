namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddProjectEntity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Projects",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    Desc = c.String(),
                    OwnerId = c.String(nullable: false, maxLength: 128),
                    WallId = c.Int(nullable: false),
                    OrganizationId = c.Int(nullable: false),
                    Created = c.DateTime(nullable: false),
                    CreatedBy = c.String(),
                    Modified = c.DateTime(nullable: false),
                    ModifiedBy = c.String(),
                    IsDeleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId, true, "FK_Org_Projects")
                .ForeignKey("dbo.AspNetUsers", t => t.OwnerId, cascadeDelete: true)
                .ForeignKey("dbo.Walls", t => t.WallId, cascadeDelete: true)
                .Index(t => t.OwnerId)
                .Index(t => t.WallId)
                .Index(t => t.OrganizationId);

            CreateTable(
                "dbo.Project2ApplicationUser",
                c => new
                {
                    Project2_Id = c.Int(nullable: false),
                    ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new { t.Project2_Id, t.ApplicationUser_Id })
                .ForeignKey("dbo.Projects", t => t.Project2_Id, cascadeDelete: false)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: false)
                .Index(t => t.Project2_Id)
                .Index(t => t.ApplicationUser_Id);

            CreateTable(
                "dbo.Project2Skill",
                c => new
                {
                    Project2_Id = c.Int(nullable: false),
                    Skill_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Project2_Id, t.Skill_Id })
                .ForeignKey("dbo.Projects", t => t.Project2_Id, cascadeDelete: true)
                .ForeignKey("dbo.Skills", t => t.Skill_Id, cascadeDelete: true)
                .Index(t => t.Project2_Id)
                .Index(t => t.Skill_Id);

            // Migrate projects, members, create walls 
            Sql(@"
                    -- Create walls
                    INSERT INTO dbo.Walls
                        (Name, OrganizationId, Created, CreatedBy, Modified, ModifiedBy, IsDeleted, Type, Access)
                    SELECT 
                        tb.Name, tb.OrganizationId, tb.Created, tb.CreatedBy, tb.Modified, tb.ModifiedBy, tb.IsDeleted, 3, 1 
                        FROM dbo.AbstractClassifiers AS tb 
                        WHERE tb.ClassificatorType = 'Project'

                    -- Insert projects and map with walls
                    INSERT INTO dbo.Projects
                        (Name, WallId, OrganizationId, OwnerId, Created, CreatedBy, Modified, ModifiedBy, IsDeleted)
                    SELECT 
                        tb.Name,
                        (SELECT TOP 1 Id FROM dbo.Walls WHERE Name = tb.Name AND Type = 3),
                        tb.OrganizationId,
                        (SELECT TOP 1 usr.Id FROM dbo.AspNetUsers AS usr WHERE usr.IsManagingDirector = 1 AND usr.IsDeleted = 0),
                        GETUTCDATE(),
                        tb.CreatedBy,
                        GETUTCDATE(),
                        tb.ModifiedBy,
                        tb.IsDeleted
                        FROM dbo.AbstractClassifiers AS tb
                        WHERE tb.ClassificatorType = 'Project'

                    -- Map users with projects
                    INSERT INTO dbo.Project2ApplicationUser
                        (ApplicationUser_Id, Project2_Id)
                    SELECT 
                        proj.UserId,
                        (SELECT TOP 1
                            Id 
                        FROM dbo.Projects 
                        WHERE Name = (SELECT TOP 1
                                        tb.Name	
                                        FROM dbo.AbstractClassifiers as tb
                                        WHERE tb.ClassificatorType = 'Project' AND tb.Id = proj.ProjectID))
                    FROM dbo.UsersProjects AS proj
                    JOIN dbo.AspNetUsers AS Users ON Users.Id = proj.UserId
                    WHERE Users.IsDeleted = 0

                    -- Add project members to wall members
                    INSERT INTO dbo.WallMembers
                        (WallId, UserId, Created, CreatedBy, Modified, ModifiedBy, IsDeleted, EmailNotificationsEnabled)
                    SELECT 
                        projs.WallId, projusr.ApplicationUser_Id, projs.Created, projs.CreatedBy, projs.Modified, projs.ModifiedBy, walls.IsDeleted, 1
                    FROM dbo.Project2ApplicationUser AS projusr
                    JOIN dbo.Projects AS projs ON projs.Id = projusr.Project2_Id
                    JOIN dbo.Walls AS walls ON walls.Id = projs.WallId

                    -- Add project owner as wall moderator
                    INSERT INTO dbo.WallModerators
                        (WallId, UserId, Created, CreatedBy, Modified, ModifiedBy, IsDeleted)
                    SELECT 
                        WallId, OwnerId, Created, CreatedBy, Modified, ModifiedBy, IsDeleted
                    FROM dbo.Projects
                ");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Projects", "WallId", "dbo.Walls");
            DropForeignKey("dbo.Project2Skill", "Skill_Id", "dbo.Skills");
            DropForeignKey("dbo.Project2Skill", "Project2_Id", "dbo.Projects");
            DropForeignKey("dbo.Projects", "OwnerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Projects", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.Project2ApplicationUser", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Project2ApplicationUser", "Project2_Id", "dbo.Projects");
            DropIndex("dbo.Project2Skill", new[] { "Skill_Id" });
            DropIndex("dbo.Project2Skill", new[] { "Project2_Id" });
            DropIndex("dbo.Project2ApplicationUser", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.Project2ApplicationUser", new[] { "Project2_Id" });
            DropIndex("dbo.Projects", new[] { "OrganizationId" });
            DropIndex("dbo.Projects", new[] { "WallId" });
            DropIndex("dbo.Projects", new[] { "OwnerId" });
            DropTable("dbo.Project2Skill");
            DropTable("dbo.Project2ApplicationUser");
            DropTable("dbo.Projects");
        }
    }
}
