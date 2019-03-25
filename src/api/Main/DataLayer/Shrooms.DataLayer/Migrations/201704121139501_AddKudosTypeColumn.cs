namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddKudosTypeColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KudosTypes", "Type", c => c.Int(nullable: false, defaultValue: 1));
        }

        public override void Down()
        {
            DropColumn("dbo.KudosTypes", "Type");
        }
    }
}
