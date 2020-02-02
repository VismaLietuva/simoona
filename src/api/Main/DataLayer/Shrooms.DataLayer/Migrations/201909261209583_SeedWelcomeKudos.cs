namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class SeedWelcomeKudos : DbMigration
    {
        public override void Up()
        {
            Sql(
                "INSERT INTO KudosTypes" +
                "(Name, Value, Created, Modified, Description, Type, IsActive)" +
                "VALUES ('Welcome', 3, GETDATE(), GETDATE(), 'Welcome Kudos for you. Be active and earn more!', 5, 0)");
        }

        public override void Down()
        {
        }
    }
}
