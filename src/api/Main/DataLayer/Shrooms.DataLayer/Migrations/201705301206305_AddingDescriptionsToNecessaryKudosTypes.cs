namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingDescriptionsToNecessaryKudosTypes : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.KudosTypes SET Description = 'kudos.typeSend' WHERE Name = 'Send'");
            Sql("UPDATE dbo.KudosTypes SET Description = 'kudos.typeMinus' WHERE Name = 'Minus'");
            Sql("UPDATE dbo.KudosTypes SET Description = 'kudos.typeOther' WHERE Name = 'Other'");
        }
        
        public override void Down()
        {
        }
    }
}
