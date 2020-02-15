namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SetEmailAsUniqueField : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE [dbo].[AspNetUsers] SET [Email] = LEFT(NEWID(), 8) + '@visma.com' WHERE Email in (SELECT[Email] FROM[Shrooms].[dbo].[AspNetUsers] GROUP BY[Email] HAVING(COUNT(Email) > 1)) and[IsDeleted] = 1 or[Email] is NULL");
            CreateIndex("dbo.AspNetUsers", "Email", unique: true, name: "Email");
        }
        
        public override void Down()
        {
            DropIndex("dbo.AspNetUsers", "Email");
        }
    }
}
