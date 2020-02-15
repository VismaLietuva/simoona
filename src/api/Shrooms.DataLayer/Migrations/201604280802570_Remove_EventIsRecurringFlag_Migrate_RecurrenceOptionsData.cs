namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Remove_EventIsRecurringFlag_Migrate_RecurrenceOptionsData : DbMigration
    {
        public override void Up()
        {
            Sql(@"BEGIN TRAN
				  DECLARE
					@NoneValue int,

					@EveryDayOldValue int,
					@EveryDayNewValue int,

					@EveryWeekOldValue int,
					@EveryWeekNewValue int,

					@EveryTwoWeeksOldValue int,
					@EveryTwoWeeksNewValue int,

					@EveryMonthOldValue int,
					@EveryMonthNewValue int

				SELECT
					@NoneValue = 0,

					@EveryDayOldValue = 0,
					@EveryDayNewValue = 1,

					@EveryWeekOldValue = 1,
					@EveryWeekNewValue = 2,

					@EveryTwoWeeksOldValue = 2,
					@EveryTwoWeeksNewValue = 3,

					@EveryMonthOldValue = 3,
					@EveryMonthNewValue = 4

				UPDATE [dbo].[Events]
					SET EventRecurring = @EveryMonthNewValue
					WHERE EventRecurring = @EveryMonthOldValue

				UPDATE [dbo].[Events]
					SET EventRecurring = @EveryTwoWeeksNewValue
					WHERE EventRecurring = @EveryTwoWeeksOldValue

				UPDATE [dbo].[Events]
					SET EventRecurring = @EveryWeekNewValue
					WHERE EventRecurring = @EveryWeekOldValue

				UPDATE [dbo].[Events]
					SET EventRecurring = @EveryDayNewValue
					WHERE EventRecurring = @EveryDayOldValue

				UPDATE [dbo].[Events]
					SET EventRecurring = @NoneValue
					WHERE IsRecurring = 0
				COMMIT TRAN");
            DropColumn("dbo.Events", "IsRecurring");
        }

        public override void Down()
        {
            AddColumn("dbo.Events", "IsRecurring", c => c.Boolean(nullable: false));
        }
    }
}
