namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuthProvidersToOrganization : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "AuthenticationProviders", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organizations", "AuthenticationProviders");
        }
    }
}
