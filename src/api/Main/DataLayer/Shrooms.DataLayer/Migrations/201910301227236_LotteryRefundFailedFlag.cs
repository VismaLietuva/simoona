namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LotteryRefundFailedFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lotteries", "RefundFailed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Lotteries", "RefundFailed");
        }
    }
}
