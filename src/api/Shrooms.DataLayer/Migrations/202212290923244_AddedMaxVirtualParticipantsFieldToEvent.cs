namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMaxVirtualParticipantsFieldToEvent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "MaxVirtualParticipants", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "MaxVirtualParticipants");
        }
    }
}
