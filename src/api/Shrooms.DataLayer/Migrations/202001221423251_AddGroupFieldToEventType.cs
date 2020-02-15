namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGroupFieldToEventType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventTypes", "SingleJoinGroupName", c => c.String(maxLength: 100, defaultValue: null));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventTypes", "SingleJoinGroupName");
        }
    }
}
