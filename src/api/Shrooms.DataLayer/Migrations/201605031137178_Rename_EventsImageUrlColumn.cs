namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Rename_EventsImageUrlColumn : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Events", "ImageUrl", "ImageName");
        }

        public override void Down()
        {
            RenameColumn("dbo.Events", "ImageName", "ImageUrl");
        }
    }
}
