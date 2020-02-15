namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RequiredUserConfirmationFlagAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "RequiresUserConfirmation", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organizations", "RequiresUserConfirmation");
        }
    }
}
