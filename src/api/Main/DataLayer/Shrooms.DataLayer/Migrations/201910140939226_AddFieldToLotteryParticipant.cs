namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFieldToLotteryParticipant : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LotteryParticipants", "IsRefunded", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LotteryParticipants", "IsRefunded");
        }
    }
}
