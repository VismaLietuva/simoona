namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsHiddenFlagToWallModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Walls", "IsHidden", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Walls", "IsHidden");
        }
    }
}
