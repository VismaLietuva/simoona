namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBaseModelToEventReminder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventReminders", "Created", c => c.DateTime(nullable: false));
            AddColumn("dbo.EventReminders", "CreatedBy", c => c.String());
            AddColumn("dbo.EventReminders", "Modified", c => c.DateTime(nullable: false));
            AddColumn("dbo.EventReminders", "ModifiedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventReminders", "ModifiedBy");
            DropColumn("dbo.EventReminders", "Modified");
            DropColumn("dbo.EventReminders", "CreatedBy");
            DropColumn("dbo.EventReminders", "Created");
        }
    }
}
