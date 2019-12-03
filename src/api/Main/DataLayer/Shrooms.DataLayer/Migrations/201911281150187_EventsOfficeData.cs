namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EventsOfficeData : DbMigration
    {
        public override void Up()
        {
            Sql(@"declare @results varchar(500)
                select @results = coalesce(@results + ',', '') + '""' + convert(varchar(12), Id) + '""'
                     from Offices
                     where IsDeleted = 0
                Update Events
                SET Offices = '[""' + convert(varchar(12), OfficeId) + '""]'
                WHERE OfficeId IS NOT NULL;
            Update Events
                SET Offices = '[' + @results + ']'
                WHERE OfficeId IS NULL;", false);
        }
        
        public override void Down()
        {
        }
    }
}
