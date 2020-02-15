namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBookAppGuidToOrganization : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "BookAppAuthorizationGuid", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organizations", "BookAppAuthorizationGuid");
        }
    }
}
