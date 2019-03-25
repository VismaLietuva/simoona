namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MultiwallEntitiesUpdates : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.WallUsers", newName: "WallMembers");
            DropForeignKey("dbo.Walls", "ParentId", "dbo.Walls");
            DropForeignKey("dbo.Posts", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.Walls", new[] { "ParentId" });
            DropIndex("dbo.Posts", new[] { "OrganizationId" });
            RenameColumn(table: "dbo.Posts", name: "ApplicationUserId", newName: "AuthorId");
            RenameColumn(table: "dbo.Comments", name: "ApplicationUserId", newName: "AuthorId");
            RenameIndex(table: "dbo.Posts", name: "IX_ApplicationUserId", newName: "IX_AuthorId");
            RenameIndex(table: "dbo.Comments", name: "IX_ApplicationUserId", newName: "IX_AuthorId");
            CreateTable(
                "dbo.WallModerators",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WallId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.WallId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Walls", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.Walls", "Access", c => c.Int(nullable: false));
            DropColumn("dbo.Walls", "ParentId");
            DropColumn("dbo.Posts", "OrganizationId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Posts", "OrganizationId", c => c.Int());
            AddColumn("dbo.Walls", "ParentId", c => c.Int());
            DropIndex("dbo.WallModerators", new[] { "UserId" });
            DropIndex("dbo.WallModerators", new[] { "WallId" });
            DropColumn("dbo.Walls", "Access");
            DropColumn("dbo.Walls", "Type");
            DropTable("dbo.WallModerators");
            RenameIndex(table: "dbo.Comments", name: "IX_AuthorId", newName: "IX_ApplicationUserId");
            RenameIndex(table: "dbo.Posts", name: "IX_AuthorId", newName: "IX_ApplicationUserId");
            RenameColumn(table: "dbo.Comments", name: "AuthorId", newName: "ApplicationUserId");
            RenameColumn(table: "dbo.Posts", name: "AuthorId", newName: "ApplicationUserId");
            CreateIndex("dbo.Posts", "OrganizationId");
            CreateIndex("dbo.Walls", "ParentId");
            AddForeignKey("dbo.Posts", "OrganizationId", "dbo.Organizations", "Id");
            AddForeignKey("dbo.Walls", "ParentId", "dbo.Walls", "Id");
            RenameTable(name: "dbo.WallMembers", newName: "WallUsers");
        }
    }
}
