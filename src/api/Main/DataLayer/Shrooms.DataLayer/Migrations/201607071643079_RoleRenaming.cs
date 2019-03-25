namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RoleRenaming : DbMigration
    {
        public override void Up()
        {
            Sql(@"update r set r.Name = 'Administration' FROM AspNetRoles r where Name = 'Administration role'");
            Sql(@"update r set r.Name = 'EventsManagement' FROM AspNetRoles r where Name = 'Events management role'");
            Sql(@"update r set r.Name = 'ServiceRequest' FROM AspNetRoles r where Name = 'Service Request role'");
        }

        public override void Down()
        {
            //not possible, since names are set in constants
        }
    }
}