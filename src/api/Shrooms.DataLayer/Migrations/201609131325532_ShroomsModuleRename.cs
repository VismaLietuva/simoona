namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ShroomsModuleRename : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ShroomsModules", newName: "Modules");
            RenameTable(name: "dbo.ShroomsModuleOrganizations", newName: "ModuleOrganizations");
            RenameColumn(table: "dbo.ModuleOrganizations", name: "ShroomsModule_Id", newName: "Module_Id");
            RenameIndex(table: "dbo.ModuleOrganizations", name: "IX_ShroomsModule_Id", newName: "IX_Module_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ModuleOrganizations", name: "IX_Module_Id", newName: "IX_ShroomsModule_Id");
            RenameColumn(table: "dbo.ModuleOrganizations", name: "Module_Id", newName: "ShroomsModule_Id");
            RenameTable(name: "dbo.ModuleOrganizations", newName: "ShroomsModuleOrganizations");
            RenameTable(name: "dbo.Modules", newName: "ShroomsModules");
        }
    }
}
