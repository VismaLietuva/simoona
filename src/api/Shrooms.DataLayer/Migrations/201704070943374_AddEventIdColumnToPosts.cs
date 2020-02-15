namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEventIdColumnToPosts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "SharedEventId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Posts", "SharedEventId");
        }
    }
}
