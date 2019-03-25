namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProvidersEmailsToAppUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "GoogleEmail", c => c.String());
            AddColumn("dbo.AspNetUsers", "FacebookEmail", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "FacebookEmail");
            DropColumn("dbo.AspNetUsers", "GoogleEmail");
        }
    }
}
