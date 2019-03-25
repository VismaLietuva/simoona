namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FeedbackRoleEvolution : DbMigration
    {
        public override void Up()
        {
            Sql(@"delete FROM [dbo].[Permissions] where Name like 'feedback_%' and Name not like 'FEEDBACK_BASIC'");
            Sql(@"update p set p.Name = 'SUPPORT_BASIC' FROM [dbo].[Permissions] p where Name = 'FEEDBACK_BASIC'");
        }
        
        public override void Down()
        {
        }
    }
}
