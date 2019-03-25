namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ShowBirthdayColumnRemoved : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "ShowBirthDay");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "ShowBirthDay", c => c.Boolean(nullable: false));
        }
    }
}
