namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddKudosYearlyMultipliersColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "KudosYearlyMultipliers", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organizations", "KudosYearlyMultipliers");
        }
    }
}
