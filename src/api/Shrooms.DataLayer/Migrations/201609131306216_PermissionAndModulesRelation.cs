namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class PermissionAndModulesRelation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Permissions", "OrganizationId", "dbo.Organizations");
            DropIndex("dbo.Permissions", new[] { "OrganizationId" });
            AddColumn("dbo.Permissions", "ModuleId", c => c.Int());
            CreateIndex("dbo.Permissions", "ModuleId");
            AddForeignKey("dbo.Permissions", "ModuleId", "dbo.ShroomsModules", "Id");
            DropColumn("dbo.Permissions", "OrganizationId");
        }

        public override void Down()
        {
            AddColumn("dbo.Permissions", "OrganizationId", c => c.Int());
            DropForeignKey("dbo.Permissions", "ModuleId", "dbo.ShroomsModules");
            DropIndex("dbo.Permissions", new[] { "ModuleId" });
            DropColumn("dbo.Permissions", "ModuleId");
            CreateIndex("dbo.Permissions", "OrganizationId");
            AddForeignKey("dbo.Permissions", "OrganizationId", "dbo.Organizations", "Id");
        }
    }
}
