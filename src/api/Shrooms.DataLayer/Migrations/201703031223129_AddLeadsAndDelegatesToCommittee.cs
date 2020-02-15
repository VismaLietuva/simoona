namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLeadsAndDelegatesToCommittee : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.CommitteeApplicationUsers", newName: "CommitteesUsersMembership");
            DropPrimaryKey("dbo.CommitteesUsersMembership");
            CreateTable(
                "dbo.CommitteesUsersDelegates",
                c => new
                    {
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                        Committee_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationUser_Id, t.Committee_Id })
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.Committees", t => t.Committee_Id, cascadeDelete: true)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.Committee_Id);
            
            CreateTable(
                "dbo.CommitteesUsersLeadership",
                c => new
                    {
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                        Committee_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationUser_Id, t.Committee_Id })
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.Committees", t => t.Committee_Id, cascadeDelete: true)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.Committee_Id);
            
            AddPrimaryKey("dbo.CommitteesUsersMembership", new[] { "ApplicationUser_Id", "Committee_Id" });
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CommitteesUsersLeadership", "Committee_Id", "dbo.Committees");
            DropForeignKey("dbo.CommitteesUsersLeadership", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.CommitteesUsersDelegates", "Committee_Id", "dbo.Committees");
            DropForeignKey("dbo.CommitteesUsersDelegates", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.CommitteesUsersLeadership", new[] { "Committee_Id" });
            DropIndex("dbo.CommitteesUsersLeadership", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.CommitteesUsersDelegates", new[] { "Committee_Id" });
            DropIndex("dbo.CommitteesUsersDelegates", new[] { "ApplicationUser_Id" });
            DropPrimaryKey("dbo.CommitteesUsersMembership");
            DropTable("dbo.CommitteesUsersLeadership");
            DropTable("dbo.CommitteesUsersDelegates");
            AddPrimaryKey("dbo.CommitteesUsersMembership", new[] { "Committee_Id", "ApplicationUser_Id" });
            RenameTable(name: "dbo.CommitteesUsersMembership", newName: "CommitteeApplicationUsers");
        }
    }
}
