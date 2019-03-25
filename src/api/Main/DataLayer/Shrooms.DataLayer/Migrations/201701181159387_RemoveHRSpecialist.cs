namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RemoveHRSpecialist : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE dbo.AspNetUsers
                SET JobPositionId = NULL
                WHERE JobPositionId = (SELECT TOP 1 Id FROM dbo.JobPositions WHERE UPPER(Title) = UPPER('HR specialist'))

                DELETE TOP (1) FROM dbo.JobPositions WHERE UPPER(Title) = UPPER('HR specialist')");
        }

        public override void Down()
        {
        }
    }
}
