namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSpeakersTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Speakers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        PhotoId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable("dbo.Speakers");
        }
    }
}
