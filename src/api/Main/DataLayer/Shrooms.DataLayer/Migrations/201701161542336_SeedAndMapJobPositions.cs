namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class SeedAndMapJobPositions : DbMigration
    {
        public override void Up()
        {
            Sql(@"INSERT INTO dbo.JobPositions VALUES ('.NET developer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Accountant', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Android developer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Application security specialist', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('BI developer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Communications specialist', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Concept developer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Delphi developer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Developer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Development department manager', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Expansion manager', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Expansion specialist', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Finance manager', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Frontend developer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('HR specialist', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('iOS developer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('IS support specialist', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('IT business analyst', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('IT systems administrator', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('IT systems architect', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('JAVA developer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Managing director', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Network engineer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Office administrator', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('PHP developer', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Project manager', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('QA', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('QA architect', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('SaaS administrator', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Software architect', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Technical QA', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)
                    INSERT INTO dbo.JobPositions VALUES('Intern', 2, GETUTCDATE(), NULL, GETUTCDATE(), NULL, 0)

                    UPDATE dbo.AspNetUsers
                    SET JobPositionId = (SELECT TOP 1 Positions.Id FROM dbo.JobPositions AS Positions WHERE UPPER(dbo.AspNetUsers.JobTitle) = UPPER(Positions.Title))
                    WHERE JobTitle IS NOT NULL");
        }

        public override void Down()
        {
        }
    }
}
