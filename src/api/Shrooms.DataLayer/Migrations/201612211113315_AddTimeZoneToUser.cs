namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddTimeZoneToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "TimeZone", c => c.String(nullable: false, defaultValueSql: "'FLE Standard Time'"));
            AddColumn("dbo.Organizations", "TimeZone", c => c.String(nullable: false, defaultValueSql: "'FLE Standard Time'"));
        }

        public override void Down()
        {
            DropColumn("dbo.Organizations", "TimeZone");
            DropColumn("dbo.AspNetUsers", "TimeZone");
        }
    }
}
