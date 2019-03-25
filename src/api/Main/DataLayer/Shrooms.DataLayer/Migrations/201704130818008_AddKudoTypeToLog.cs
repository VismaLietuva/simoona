namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddKudoTypeToLog : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KudosLogs", "KudosTypeType", c => c.Int(nullable: false, defaultValue: 1));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KudosLogs", "KudosTypeType");
        }
    }
}
