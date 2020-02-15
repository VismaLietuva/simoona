namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedPostEventReferenceAndAddedRestrictions : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Posts", "EventId", "dbo.Events");
            DropForeignKey("dbo.Events", "WallId", "dbo.Walls");
            DropForeignKey("dbo.Posts", "WallId", "dbo.Walls");
            DropIndex("dbo.Events", new[] { "WallId" });
            DropIndex("dbo.Posts", new[] { "EventId" });
            DropIndex("dbo.Posts", new[] { "WallId" });
            AlterColumn("dbo.Events", "WallId", c => c.Int(nullable: false));
            AlterColumn("dbo.Posts", "WallId", c => c.Int(nullable: false));
            CreateIndex("dbo.Events", "WallId");
            CreateIndex("dbo.Posts", "WallId");
            AddForeignKey("dbo.Events", "WallId", "dbo.Walls", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Posts", "WallId", "dbo.Walls", "Id", cascadeDelete: true);
            DropColumn("dbo.Posts", "EventId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Posts", "EventId", c => c.Guid());
            DropForeignKey("dbo.Posts", "WallId", "dbo.Walls");
            DropForeignKey("dbo.Events", "WallId", "dbo.Walls");
            DropIndex("dbo.Posts", new[] { "WallId" });
            DropIndex("dbo.Events", new[] { "WallId" });
            AlterColumn("dbo.Posts", "WallId", c => c.Int());
            AlterColumn("dbo.Events", "WallId", c => c.Int());
            CreateIndex("dbo.Posts", "WallId");
            CreateIndex("dbo.Posts", "EventId");
            CreateIndex("dbo.Events", "WallId");
            AddForeignKey("dbo.Posts", "WallId", "dbo.Walls", "Id");
            AddForeignKey("dbo.Events", "WallId", "dbo.Walls", "Id");
            AddForeignKey("dbo.Posts", "EventId", "dbo.Events", "Id");
        }
    }
}
