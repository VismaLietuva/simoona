namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RandomUserDistributionToWalls : DbMigration
    {
        public override void Up()
        {
            Sql(@"
                DECLARE @min int = (SELECT MIN(Id) FROM dbo.Walls WHERE ParentId IS NOT null), @max int = (SELECT MAX(Id) FROM dbo.Walls WHERE ParentId IS NOT null) + 1
                DECLARE @timestamp datetime = GETUTCDATE()

                INSERT INTO WallUsers (WallId, UserId, Created, Modified, IsDeleted)
                SELECT FLOOR(RAND(CHECKSUM(NEWID()))*(@max-@min)+@min) AS WallId, Id, @timestamp AS Created, @timestamp as Modified, IsDeleted 
                FROM AspNetUsers");
        }

        public override void Down()
        {
        }
    }
}
