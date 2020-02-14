namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RoomToConfirmRemoval : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "RoomToConfirmId", "dbo.Rooms");
            DropIndex("dbo.AspNetUsers", new[] { "RoomToConfirmId" });
            DropColumn("dbo.AspNetUsers", "RoomToConfirmId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "RoomToConfirmId", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "RoomToConfirmId");
            AddForeignKey("dbo.AspNetUsers", "RoomToConfirmId", "dbo.Rooms", "Id");
        }
    }
}
