namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedJoinOptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "AllowMaybeGoing", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.Events", "AllowNotGoing", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "AllowNotGoing");
            DropColumn("dbo.Events", "AllowMaybeGoing");
        }
    }
}
