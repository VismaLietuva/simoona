using System.Data.Entity.Migrations;
using Shrooms.DataLayer.DAL;

namespace Shrooms.DataLayer.Migrations
{
    public partial class UpdateTypeOfSpecialKudos : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.KudosLogs SET KudosTypeType = " + 2 + " WHERE KudosTypeName = 'Send'");
            Sql("UPDATE dbo.KudosLogs SET KudosTypeType = " + 3 + " WHERE KudosTypeName = 'Minus'");
            Sql("UPDATE dbo.KudosLogs SET KudosTypeType = " + 4 + " WHERE KudosTypeName = 'Other'");

            Sql("UPDATE dbo.KudosTypes SET Type = " + 2 + " WHERE Name = 'Send'");
            Sql("UPDATE dbo.KudosTypes SET Type = " + 3 + " WHERE Name = 'Minus'");
            Sql("UPDATE dbo.KudosTypes SET Type = " + 4 + " WHERE Name = 'Other'");
        }

        public override void Down()
        {
            Sql("UPDATE dbo.KudosLogs SET KudosTypeType = " + 1);
            Sql("UPDATE dbo.KudosTypes SET Type = " + 1);
        }
    }
}
