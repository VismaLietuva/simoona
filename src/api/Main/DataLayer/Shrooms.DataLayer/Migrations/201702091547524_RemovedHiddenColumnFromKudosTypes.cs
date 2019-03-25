namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedHiddenColumnFromKudosTypes : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.KudosTypes", "Hidden");
        }
        
        public override void Down()
        {
            AddColumn("dbo.KudosTypes", "Hidden", c => c.Boolean(nullable: false));
        }
    }
}
