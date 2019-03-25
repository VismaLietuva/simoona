namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Add_EventsRegistrationDeadlineDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "RegistrationDeadline", c => c.DateTime(nullable: false, defaultValue: DateTime.UtcNow));
            Sql(@"UPDATE [dbo].[Events]
                  SET RegistrationDeadline = StartDate");
        }

        public override void Down()
        {
            DropColumn("dbo.Events", "RegistrationDeadline");
        }
    }
}
