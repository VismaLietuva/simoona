namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameBackgroundJobColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BackgroundJobs", "ExternalId", c => c.String());
            DropColumn("dbo.BackgroundJobs", "JobGuid");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BackgroundJobs", "JobGuid", c => c.String());
            DropColumn("dbo.BackgroundJobs", "ExternalId");
        }
    }
}
