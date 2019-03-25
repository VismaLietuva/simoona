namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPictureIdToKudosShopItems : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KudosShopItems", "PictureId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.KudosShopItems", "PictureId");
        }
    }
}
