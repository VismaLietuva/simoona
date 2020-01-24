namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLotteryAndLotteryParticipant : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Lotteries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        EndDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        EntryFee = c.Int(nullable: false),
                        Images = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId);
            
            CreateTable(
                "dbo.LotteryParticipants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LotteryId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        Joined = c.DateTime(nullable: false),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        Modified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lotteries", t => t.LotteryId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.LotteryId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LotteryParticipants", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.LotteryParticipants", "LotteryId", "dbo.Lotteries");
            DropForeignKey("dbo.Lotteries", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.LotteryParticipants", new[] { "UserId" });
            DropIndex("dbo.LotteryParticipants", new[] { "LotteryId" });
            DropIndex("dbo.Lotteries", new[] { "OrganizationId" });
            DropTable("dbo.LotteryParticipants");
            DropTable("dbo.Lotteries");
        }
    }
}
