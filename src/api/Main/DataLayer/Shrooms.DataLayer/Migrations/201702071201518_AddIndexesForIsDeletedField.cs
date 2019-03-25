namespace Shrooms.DataLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexesForIsDeletedField : DbMigration
    {
        public override void Up()
        {
            Sql(@"CREATE NONCLUSTERED INDEX[nci_wi_WallMembers_6C8CE6B55B79BC00FDA53D9B579C2EFA] 
                  ON[dbo].[WallMembers]([IsDeleted], [UserId])");

            Sql(@"CREATE NONCLUSTERED INDEX[nci_wi_EventParticipants_CA1F6B4699FAB2347B166CEA9639C7E8] 
                  ON[dbo].[EventParticipants] ([IsDeleted]) INCLUDE([EventId])");
        }
        
        public override void Down()
        {
            Sql(@"DROP INDEX [nci_wi_WallMembers_6C8CE6B55B79BC00FDA53D9B579C2EFA]
                  ON[dbo].[WallMembers]");

            Sql(@"DROP INDEX [nci_wi_EventParticipants_CA1F6B4699FAB2347B166CEA9639C7E8]
                  ON[dbo].[EventParticipants]");
        }
    }
}
