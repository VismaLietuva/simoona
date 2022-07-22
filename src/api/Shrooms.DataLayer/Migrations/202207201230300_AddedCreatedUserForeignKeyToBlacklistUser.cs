namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCreatedUserForeignKeyToBlacklistUser : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.BlacklistUsers", "CreatedBy", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.BlacklistUsers", "CreatedBy");
            AddForeignKey("dbo.BlacklistUsers", "CreatedBy", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlacklistUsers", "CreatedBy", "dbo.AspNetUsers");
            DropIndex("dbo.BlacklistUsers", new[] { "CreatedBy" });
            AlterColumn("dbo.BlacklistUsers", "CreatedBy", c => c.String());
        }
    }
}
