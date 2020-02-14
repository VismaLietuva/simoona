namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitEventWallsPermissions : DbMigration
    {
        public override void Up()
        {
            // Empty migration to trigger PermissionInitializer to initialize new roles
        }
        
        public override void Down()
        {
        }
    }
}
