namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEventOptionRulesToAnEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventOptions", "Rule", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventOptions", "Rule");
        }
    }
}
