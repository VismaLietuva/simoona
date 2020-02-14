namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddColumnIsManagingUserToApplicationUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsManagingDirector", c => c.Boolean(nullable: false));
            Sql("UPDATE [dbo].[AspNetUsers] SET [IsManagingDirector] = 'true' WHERE[UserName] = 'urbonman'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IsManagingDirector");
        }
    }
}
