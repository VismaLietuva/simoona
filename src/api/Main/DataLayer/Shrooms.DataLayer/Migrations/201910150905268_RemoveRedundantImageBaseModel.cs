namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRedundantImageBaseModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LotteryParticipants", "Joined", c => c.DateTime(nullable: false));
            DropColumn("dbo.LotteryParticipants", "Entered");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LotteryParticipants", "Entered", c => c.DateTime(nullable: false));
            DropColumn("dbo.LotteryParticipants", "Joined");
        }
    }
}
