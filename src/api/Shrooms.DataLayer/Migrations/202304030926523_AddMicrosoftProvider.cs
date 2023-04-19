namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMicrosoftProvider : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "MicrosoftEmail", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "MicrosoftEmail");
        }
    }
}
