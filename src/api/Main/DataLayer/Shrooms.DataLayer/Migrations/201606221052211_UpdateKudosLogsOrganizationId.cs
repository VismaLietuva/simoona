namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateKudosLogsOrganizationId : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE KudosLogs SET OrganizationId = 2");
            DropForeignKey("dbo.KudosLogs", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.KudosLogs", new[] { "OrganizationId" });
            AlterColumn("dbo.KudosLogs", "Comments", c => c.String(nullable: false));
            AlterColumn("dbo.KudosLogs", "OrganizationId", c => c.Int(nullable: false));
            CreateIndex("dbo.KudosLogs", "OrganizationId");
            AddForeignKey("dbo.KudosLogs", "OrganizationId", "dbo.Organizations", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.KudosLogs", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.KudosLogs", new[] { "OrganizationId" });
            AlterColumn("dbo.KudosLogs", "OrganizationId", c => c.Int());
            AlterColumn("dbo.KudosLogs", "Comments", c => c.String());
            CreateIndex("dbo.KudosLogs", "OrganizationId");
            AddForeignKey("dbo.KudosLogs", "OrganizationId", "dbo.Organizations", "Id");
        }
    }
}
