namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropColumnIsRequiredForBundleFromModules : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Modules", "IsRequiredForBundle");
            Sql("DELETE FROM[dbo].[Modules]");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Modules", "IsRequiredForBundle", c => c.Boolean(nullable: false));
        }
    }
}
