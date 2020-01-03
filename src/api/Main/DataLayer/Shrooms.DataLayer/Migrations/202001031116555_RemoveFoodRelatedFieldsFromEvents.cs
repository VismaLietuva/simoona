namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveFoodRelatedFieldsFromEvents : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Events", "FoodOption");
            DropColumn("dbo.EventTypes", "IsFoodRelated");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EventTypes", "IsFoodRelated", c => c.Boolean(nullable: false));
            AddColumn("dbo.Events", "FoodOption", c => c.Int());
        }
    }
}
