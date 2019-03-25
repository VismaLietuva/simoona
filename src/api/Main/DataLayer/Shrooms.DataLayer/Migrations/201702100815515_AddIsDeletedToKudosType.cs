namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsDeletedToKudosType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KudosTypes", "IsDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KudosTypes", "IsDeleted");
        }
    }
}
