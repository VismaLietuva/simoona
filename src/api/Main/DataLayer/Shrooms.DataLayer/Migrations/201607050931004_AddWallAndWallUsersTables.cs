namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWallAndWallUsersTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Walls",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        ParentId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId)
                .ForeignKey("dbo.Walls", t => t.ParentId)
                .Index(t => t.ParentId)
                .Index(t => t.OrganizationId);
            
            CreateTable(
                "dbo.WallUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WallId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.Walls", t => t.WallId, cascadeDelete: true)
                .Index(t => t.WallId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Posts", "WallId", c => c.Int());
            CreateIndex("dbo.Posts", "WallId");
            AddForeignKey("dbo.Posts", "WallId", "dbo.Walls", "Id");

            Sql(@"INSERT INTO[dbo].[Walls]
                    ([Name]
                    ,[Description]
                    ,[OrganizationId]
                    ,[Created]
                    ,[CreatedBy]
                    ,[Modified]
                    ,[ModifiedBy]
                    ,[IsDeleted])
                 VALUES
                    ('Simoona', '', 2, GETUTCDATE(), '', GETUTCDATE(), '', '0'),
		            ('Girls','',2,GETUTCDATE(),'',GETUTCDATE(),'','0'),
		            ('GrumpyCat','',2,GETUTCDATE(),'',GETUTCDATE(),'','0'),
		            ('Žvejai','',2,GETUTCDATE(),'',GETUTCDATE(),'','0'),
		            ('Geeks','',2,GETUTCDATE(),'',GETUTCDATE(),'','0')
                    GO");

            Sql(@"UPDATE[dbo].[Walls]
                SET[ParentId] = (SELECT[Id]
                    FROM[dbo].[Walls]
                    Where Name = 'Simoona')
                WHERE Name != 'Simoona'
                GO");

            Sql(@"UPDATE[dbo].[Posts]
                SET[WallId] = (SELECT[Id]
                    FROM [dbo].[Walls]
                    Where Name = 'Simoona')
                WHERE EventId is null
                GO");

            Sql(@"INSERT INTO[dbo].[WallUsers]
                    ([WallId]
                    ,[UserId]
                    ,[Created]
                    ,[CreatedBy]
                    ,[Modified]
                    ,[ModifiedBy]
                    ,[IsDeleted])

                SELECT
                    (SELECT [Id] FROM [dbo].[Walls] Where Name = 'Simoona'), 
                    [Id], 
                    GETUTCDATE(), 
                    '', 
                    GETUTCDATE(), 
                    '',
                    [IsDeleted]
                FROM [dbo].[AspNetUsers]

                GO");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WallUsers", "WallId", "dbo.Walls");
            DropForeignKey("dbo.WallUsers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Posts", "WallId", "dbo.Walls");
            DropForeignKey("dbo.Walls", "ParentId", "dbo.Walls");
            DropForeignKey("dbo.Walls", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.WallUsers", new[] { "UserId" });
            DropIndex("dbo.WallUsers", new[] { "WallId" });
            DropIndex("dbo.Walls", new[] { "OrganizationId" });
            DropIndex("dbo.Walls", new[] { "ParentId" });
            DropIndex("dbo.Posts", new[] { "WallId" });
            DropColumn("dbo.Posts", "WallId");
            DropTable("dbo.WallUsers");
            DropTable("dbo.Walls");
        }
    }
}
