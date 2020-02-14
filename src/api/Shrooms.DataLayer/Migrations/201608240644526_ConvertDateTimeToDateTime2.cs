namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConvertDateTimeToDateTime2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ExternalLinks", "Created", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.ExternalLinks", "Modified", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ExternalLinks", "Modified", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ExternalLinks", "Created", c => c.DateTime(nullable: false));
        }
    }
}
