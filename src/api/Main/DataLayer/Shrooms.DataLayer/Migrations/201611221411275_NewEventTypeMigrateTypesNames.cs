namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class NewEventTypeMigrateTypesNames : DbMigration
    {
        public override void Up()
        {
            Sql(@"
				BEGIN TRANSACTION
                    UPDATE dbo.EventTypes
					SET Name = N'Code Monkeys'
					WHERE Name = 'codemonkeysEventType'

					UPDATE dbo.EventTypes
					SET Name = 'Sport'
					WHERE Name = 'sportsEventType'

					UPDATE dbo.EventTypes
					SET Name = 'Food'
					WHERE Name = 'foodEventType'

					UPDATE dbo.EventTypes
					SET Name = 'Leisure'
					WHERE Name = 'leisureEventType'

                    GO

					IF NOT EXISTS (SELECT * FROM dbo.EventTypes WHERE Name = 'Hub')
					BEGIN
						INSERT INTO dbo.EventTypes
						(Name, IsSingleJoin, OrganizationId, Created, CreatedBy, Modified, ModifiedBy, IsDeleted)
						VALUES
						('Hub', 0, 2, GETUTCDATE(), null, GETUTCDATE(), null, 0)
					END
				COMMIT");
        }
    }
}
