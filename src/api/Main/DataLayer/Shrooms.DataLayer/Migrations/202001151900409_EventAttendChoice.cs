namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EventAttendChoice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventParticipants", "AttendStatus", c => c.Int(nullable: false, defaultValue: 1));
            AddColumn("dbo.EventParticipants", "AttendComment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventParticipants", "AttendComment");
            DropColumn("dbo.EventParticipants", "AttendStatus");
        }
    }
}
