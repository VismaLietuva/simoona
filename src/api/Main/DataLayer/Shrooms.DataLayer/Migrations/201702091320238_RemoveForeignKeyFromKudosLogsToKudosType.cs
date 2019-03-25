namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveForeignKeyFromKudosLogsToKudosType : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.KudosLogs", "TypeId", "dbo.KudosTypes");
            DropIndex("dbo.KudosLogs", new[] { "TypeId" });
            Sql(@"DROP INDEX KudosLogsStatsWidget ON [dbo].[KudosLogs]");
            AlterColumn("dbo.KudosLogs", "KudosTypeValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.KudosLogs", "TypeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.KudosLogs", "TypeId", c => c.Int(nullable: true));
            AlterColumn("dbo.KudosLogs", "KudosTypeValue", c => c.Int(nullable: false));
            CreateIndex("dbo.KudosLogs", "TypeId");
            AddForeignKey("dbo.KudosLogs", "TypeId", "dbo.KudosTypes", "Id", cascadeDelete: true);
        }
    }
}
