namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Events_MaxChoices_DataMigration : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE [dbo].[Events]
				SET MaxChoices = 0
				FROM [dbo].[Events] AS Eventss
				LEFT JOIN [dbo].[EventOptions] AS EventOptions 
					ON EventOptions.EventId = Eventss.Id
				WHERE 
					EventOptions.Id IS NULL");
        }

        public override void Down()
        {
        }
    }
}
