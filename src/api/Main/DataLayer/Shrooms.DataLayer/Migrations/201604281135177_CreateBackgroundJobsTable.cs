namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class CreateBackgroundJobsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BackgroundJobs",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    JobGuid = c.String(nullable: false),
                    JobType = c.Int(nullable: false),
                    Created = c.DateTime(nullable: false),
                    CreatedBy = c.String(),
                    Modified = c.DateTime(nullable: false),
                    ModifiedBy = c.String(),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Events", "BackgroundJobId", c => c.Int());
            CreateIndex("dbo.Events", "BackgroundJobId");
            AddForeignKey("dbo.Events", "BackgroundJobId", "dbo.BackgroundJobs", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Events", "BackgroundJobId", "dbo.BackgroundJobs");
            DropIndex("dbo.Events", new[] { "BackgroundJobId" });
            DropColumn("dbo.Events", "BackgroundJobId");
            DropTable("dbo.BackgroundJobs");
        }
    }
}
