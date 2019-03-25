namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RemoveWallMembers : DbMigration
    {
        public override void Up()
        {
            // Remove wall members that point to deleted user
            Sql(@"UPDATE dbo.WallMembers
                  SET IsDeleted = 1
                  FROM [dbo].[WallMembers] AS Members
                  JOIN dbo.AspNetUsers AS Users ON Users.Id = Members.UserId
                  WHERE Users.IsDeleted = 1 AND Members.IsDeleted = 0");
        }
    }
}
