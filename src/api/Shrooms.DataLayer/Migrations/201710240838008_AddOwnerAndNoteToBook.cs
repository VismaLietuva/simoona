namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOwnerAndNoteToBook : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Books", "ApplicationUserId", c => c.String(maxLength: 128));
            AddColumn("dbo.Books", "Note", c => c.String());
            AlterColumn("dbo.Books", "Code", c => c.String(maxLength: 20));
            CreateIndex("dbo.Books", "ApplicationUserId");
            AddForeignKey("dbo.Books", "ApplicationUserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Books", "ApplicationUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Books", new[] { "ApplicationUserId" });
            AlterColumn("dbo.Books", "Code", c => c.String(nullable: false, maxLength: 20));
            DropColumn("dbo.Books", "Note");
            DropColumn("dbo.Books", "ApplicationUserId");
        }
    }
}
