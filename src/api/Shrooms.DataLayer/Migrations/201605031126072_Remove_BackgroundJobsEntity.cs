namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_BackgroundJobsEntity : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Events", "BackgroundJobId", "dbo.BackgroundJobs");
            DropIndex("dbo.Events", new[] { "BackgroundJobId" });
            DropColumn("dbo.Events", "BackgroundJobId");
            DropTable("dbo.BackgroundJobs");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.BackgroundJobs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalId = c.String(),
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
    }
}
