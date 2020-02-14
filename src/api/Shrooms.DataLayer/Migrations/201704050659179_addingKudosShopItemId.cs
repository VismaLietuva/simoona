namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

#pragma warning disable SA1300 // Element must begin with upper-case letter
    public partial class addingKudosShopItemId : DbMigration
#pragma warning restore SA1300 // Element must begin with upper-case letter
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceRequests", "KudosShopItemId", c => c.Int());
            CreateIndex("dbo.ServiceRequests", "KudosShopItemId");
            AddForeignKey("dbo.ServiceRequests", "KudosShopItemId", "dbo.KudosShopItems", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ServiceRequests", "KudosShopItemId", "dbo.KudosShopItems");
            DropIndex("dbo.ServiceRequests", new[] { "KudosShopItemId" });
            DropColumn("dbo.ServiceRequests", "KudosShopItemId");
        }
    }
}
