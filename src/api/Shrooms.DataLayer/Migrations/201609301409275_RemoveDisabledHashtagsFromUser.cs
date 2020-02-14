namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveDisabledHashtagsFromUser : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.HashtagApplicationUsers", "Hashtag_Id", "dbo.Hashtags");
            DropForeignKey("dbo.HashtagApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.HashtagApplicationUsers", new[] { "Hashtag_Id" });
            DropIndex("dbo.HashtagApplicationUsers", new[] { "ApplicationUser_Id" });
            DropTable("dbo.HashtagApplicationUsers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.HashtagApplicationUsers",
                c => new
                    {
                        Hashtag_Id = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Hashtag_Id, t.ApplicationUser_Id });
            
            CreateIndex("dbo.HashtagApplicationUsers", "ApplicationUser_Id");
            CreateIndex("dbo.HashtagApplicationUsers", "Hashtag_Id");
            AddForeignKey("dbo.HashtagApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.HashtagApplicationUsers", "Hashtag_Id", "dbo.Hashtags", "Id", cascadeDelete: true);
        }
    }
}
