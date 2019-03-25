namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class KudosLogsStats : DbMigration
    {
        public override void Up()
        {
            Sql(@"CREATE NONCLUSTERED INDEX KudosLogsStatsWidget ON [dbo].[KudosLogs] ([Status],[KudosBasketId],[OrganizationId],[Created]) INCLUDE ([Points],[EmployeeId],[TypeId])");
        }
        
        public override void Down()
        {
            Sql(@"DROP INDEX KudosLogsStats ON [dbo].[KudosLogs]");
        }
    }
}