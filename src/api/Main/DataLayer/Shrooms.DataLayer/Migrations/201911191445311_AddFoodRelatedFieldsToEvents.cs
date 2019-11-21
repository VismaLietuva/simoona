namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFoodRelatedFieldsToEvents : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "FoodOption", c => c.Int());
            AddColumn("dbo.EventTypes", "IsFoodRelated", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventTypes", "IsFoodRelated");
            DropColumn("dbo.Events", "FoodOption");
        }
    }
}
