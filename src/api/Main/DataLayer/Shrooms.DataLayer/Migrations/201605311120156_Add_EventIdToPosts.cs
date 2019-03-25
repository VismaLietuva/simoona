namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_EventIdToPosts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "EventId", c => c.Guid());
            CreateIndex("dbo.Posts", "EventId");
            AddForeignKey("dbo.Posts", "EventId", "dbo.Events", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Posts", "EventId", "dbo.Events");
            DropIndex("dbo.Posts", new[] { "EventId" });
            DropColumn("dbo.Posts", "EventId");
        }
    }
}
