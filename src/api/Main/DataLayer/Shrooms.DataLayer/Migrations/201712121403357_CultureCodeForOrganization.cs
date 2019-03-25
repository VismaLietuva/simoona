namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CultureCodeForOrganization : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "CultureCode", c => c.String(defaultValue: "en-US"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organizations", "CultureCode");
        }
    }
}
