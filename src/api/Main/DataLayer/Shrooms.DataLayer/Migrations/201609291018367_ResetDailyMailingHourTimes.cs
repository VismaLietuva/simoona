namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ResetDailyMailingHourTimes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "DailyMailingHour", c => c.Time(precision: 0));
            DropColumn("dbo.AspNetUsers", "ShroomsInfoMailingHour");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "ShroomsInfoMailingHour", c => c.Time(precision: 0));
            DropColumn("dbo.AspNetUsers", "DailyMailingHour");
        }
    }
}
