namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveJobTitleFromAppUser : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "JobTitle");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "JobTitle", c => c.String());
        }
    }
}
