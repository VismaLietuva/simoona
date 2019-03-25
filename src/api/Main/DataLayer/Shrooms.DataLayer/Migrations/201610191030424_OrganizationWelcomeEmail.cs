namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class OrganizationWelcomeEmail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "WelcomeEmail", c => c.String(nullable: false, maxLength: 10000, defaultValue: @"<p style=""text-align:center; font-size:14px; font-weight:400; margin: 0 0 0 0; "">Administrator has confirmed your registration</p>"));
        }

        public override void Down()
        {
            DropColumn("dbo.Organizations", "WelcomeEmail");
        }
    }
}
