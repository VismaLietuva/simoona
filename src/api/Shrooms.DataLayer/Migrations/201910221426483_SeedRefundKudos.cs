namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeedRefundKudos : DbMigration
    {
        public override void Up()
        {
            Sql(
                "INSERT INTO KudosTypes" +
                "(Name, Value, Created, Modified, Description, Type, IsActive)" +
                "VALUES ('Refund', 1, GETDATE(), GETDATE(), 'Refunded kudos for aborted lottery', 6, 0)");
        }

        public override void Down()
        {
        }
    }
}
