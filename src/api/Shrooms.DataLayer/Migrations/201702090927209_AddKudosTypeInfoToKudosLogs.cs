namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddKudosTypeInfoToKudosLogs : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KudosLogs", "KudosTypeName", c => c.String());
            AddColumn("dbo.KudosLogs", "KudosTypeValue", c => c.Int(nullable: true));

            Sql(@"UPDATE [dbo].[KudosLogs]
                  SET [KudosTypeName] = kTypes.Name
                     ,[KudosTypeValue] = kTypes.Value
                  FROM [dbo].[KudosLogs] as logs
                  INNER JOIN [dbo].[KudosTypes] as kTypes
                  ON logs.TypeId = kTypes.Id");
        }
        
        public override void Down()
        {
            DropColumn("dbo.KudosLogs", "KudosTypeValue");
            DropColumn("dbo.KudosLogs", "KudosTypeName");
        }
    }
}
