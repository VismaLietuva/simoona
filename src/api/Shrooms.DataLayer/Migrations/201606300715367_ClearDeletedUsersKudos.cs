namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ClearDeletedUsersKudos : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE [dbo].[AspNetUsers] 
                SET RemainingKudos = 0,
                    SpentKudos = 0,
                    TotalKudos = 0
                WHERE IsDeleted = 1");
        }

        public override void Down()
        {
        }
    }
}
