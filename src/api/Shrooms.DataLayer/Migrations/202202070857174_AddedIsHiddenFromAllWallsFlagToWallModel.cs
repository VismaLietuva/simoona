namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsHiddenFromAllWallsFlagToWallModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Walls", "IsHiddenFromAllWalls", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Walls", "IsHiddenFromAllWalls");
        }
    }
}
