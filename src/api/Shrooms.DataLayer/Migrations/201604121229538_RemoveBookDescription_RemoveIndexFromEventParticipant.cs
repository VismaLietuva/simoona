namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveBookDescription_RemoveIndexFromEventParticipant : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.EventParticipants", "ix_created_date");
            DropColumn("dbo.Books", "Description");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Books", "Description", c => c.String());
            CreateIndex("dbo.EventParticipants", "Created", name: "ix_created_date");
        }
    }
}
