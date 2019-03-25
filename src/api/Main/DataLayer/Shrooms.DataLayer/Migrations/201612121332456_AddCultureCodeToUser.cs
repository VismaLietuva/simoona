namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddCultureCodeToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "CultureCode", c => c.String(nullable: false, defaultValueSql: "'en-US'"));
        }

        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "CultureCode");
        }
    }
}
