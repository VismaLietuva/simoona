namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Add_RefreshTokenEntity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RefreshTokens",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Subject = c.String(nullable: false, maxLength: 70),
                    IssuedUtc = c.DateTime(nullable: false),
                    ExpiresUtc = c.DateTime(nullable: false),
                    ProtectedTicket = c.String(nullable: false),
                    OrganizationId = c.Int(),
                    Created = c.DateTime(nullable: false),
                    CreatedBy = c.String(),
                    Modified = c.DateTime(nullable: false),
                    ModifiedBy = c.String(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId)
                .Index(t => t.Subject, unique: true)
                .Index(t => t.OrganizationId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.RefreshTokens", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.RefreshTokens", new[] { "OrganizationId" });
            DropIndex("dbo.RefreshTokens", new[] { "Subject" });
            DropTable("dbo.RefreshTokens");
        }
    }
}
