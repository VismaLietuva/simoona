namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsDeletedToKudosShopItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KudosShopItems", "IsDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KudosShopItems", "IsDeleted");
        }
    }
}
