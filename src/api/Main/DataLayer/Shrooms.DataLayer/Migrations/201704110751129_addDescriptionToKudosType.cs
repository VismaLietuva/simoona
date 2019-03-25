namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

#pragma warning disable SA1300 // Element must begin with upper-case letter
    public partial class addDescriptionToKudosType : DbMigration
#pragma warning restore SA1300 // Element must begin with upper-case letter
    {
        public override void Up()
        {
            AddColumn("dbo.KudosTypes", "Description", c => c.String(maxLength: 500));
        }

        public override void Down()
        {
            DropColumn("dbo.KudosTypes", "Description");
        }
    }
}
