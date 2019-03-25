namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameKudosLofFieldType : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.KudosLogs", "KudosTypeType", "KudosSystemType");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.KudosLogs", "KudosSystemType", "KudosTypeType");
        }
    }
}
