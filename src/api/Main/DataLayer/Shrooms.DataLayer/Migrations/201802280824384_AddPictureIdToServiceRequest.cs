namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPictureIdToServiceRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KudosLogs", "PictureId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.KudosLogs", "PictureId");
        }
    }
}
