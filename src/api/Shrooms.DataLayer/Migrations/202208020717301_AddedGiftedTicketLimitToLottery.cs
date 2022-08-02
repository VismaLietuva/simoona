namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedGiftedTicketLimitToLottery : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lotteries", "GiftedTicketLimit", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Lotteries", "GiftedTicketLimit");
        }
    }
}
