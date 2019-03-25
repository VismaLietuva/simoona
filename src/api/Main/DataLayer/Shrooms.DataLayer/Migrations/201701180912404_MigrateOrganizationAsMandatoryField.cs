namespace Shrooms.DataLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MigrateOrganizationAsMandatoryField : DbMigration
    {
        public override void Up()
        {
            Sql(@"IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Monitors'))
                BEGIN
                    UPDATE dbo.Monitors
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Floors'))
                BEGIN
                    UPDATE dbo.Floors
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Walls'))
                BEGIN
                    UPDATE dbo.Walls
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Offices'))
                BEGIN
                    UPDATE dbo.Offices
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'WorkingHours'))
                BEGIN
                    UPDATE dbo.WorkingHours
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Committees'))
                BEGIN
                    UPDATE dbo.Committees
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Rooms'))
                BEGIN
                    UPDATE dbo.Rooms
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'AspNetUsers'))
                BEGIN
                    UPDATE dbo.AspNetUsers
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'AspNetRoles'))
                BEGIN
                    UPDATE dbo.AspNetRoles
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Books'))
                BEGIN
                    UPDATE dbo.Books
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Exams'))
                BEGIN
                    UPDATE dbo.Exams
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'BookLogs'))
                BEGIN
                    UPDATE dbo.BookLogs
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'BookOffices'))
                BEGIN
                    UPDATE dbo.BookOffices
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Pictures'))
                BEGIN
                    UPDATE dbo.Pictures
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'AbstractClassifiers'))
                BEGIN
                    UPDATE dbo.AbstractClassifiers
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Events'))
                BEGIN
                    UPDATE dbo.Events
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'RoomTypes'))
                BEGIN
                    UPDATE dbo.RoomTypes
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'KudosLogs'))
                BEGIN
                    UPDATE dbo.KudosLogs
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'SyncTokens'))
                BEGIN
                    UPDATE dbo.SyncTokens
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'KudosBaskets'))
                BEGIN
                    UPDATE dbo.KudosBaskets
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'ExternalLinks'))
                BEGIN
                    UPDATE dbo.ExternalLinks
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'Pages'))
                BEGIN
                    UPDATE dbo.Pages
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'EventTypes'))
                BEGIN
                    UPDATE dbo.EventTypes
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'RefreshTokens'))
                BEGIN
                    UPDATE dbo.RefreshTokens
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'ServiceRequests'))
                BEGIN
                    UPDATE dbo.ServiceRequests
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'QualificationLevels'))
                BEGIN
                    UPDATE dbo.QualificationLevels
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END
                IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'OrganizationId' AND Object_ID = OBJECT_ID(N'ServiceRequestComments'))
                BEGIN
                    UPDATE dbo.ServiceRequestComments
                    SET OrganizationId = 2
                    WHERE OrganizationId IS NULL
                END");

            DropIndex("dbo.AspNetRoles", "RoleNameIndex2");

            DropIndex("dbo.AspNetUsers", new[] { "OrganizationId" });
            DropIndex("dbo.AbstractClassifiers", new[] { "OrganizationId" });
            DropIndex("dbo.Exams", new[] { "OrganizationId" });
            DropIndex("dbo.Committees", new[] { "OrganizationId" });
            DropIndex("dbo.QualificationLevels", new[] { "OrganizationId" });
            DropIndex("dbo.Rooms", new[] { "OrganizationId" });
            DropIndex("dbo.Floors", new[] { "OrganizationId" });
            DropIndex("dbo.Offices", new[] { "OrganizationId" });
            DropIndex("dbo.Pictures", new[] { "OrganizationId" });
            DropIndex("dbo.RoomTypes", new[] { "OrganizationId" });
            DropIndex("dbo.WorkingHours", new[] { "OrganizationId" });
            DropIndex("dbo.ExternalLinks", new[] { "OrganizationId" });
            DropIndex("dbo.KudosBaskets", new[] { "OrganizationId" });
            DropIndex("dbo.Pages", new[] { "OrganizationId" });
            DropIndex("dbo.AspNetRoles", new[] { "OrganizationId" });
            DropIndex("dbo.RefreshTokens", new[] { "OrganizationId" });
            DropIndex("dbo.ServiceRequestComments", new[] { "OrganizationId" });
            DropIndex("dbo.ServiceRequests", new[] { "OrganizationId" });
            DropIndex("dbo.SyncTokens", new[] { "OrganizationId" });
            AlterColumn("dbo.AspNetUsers", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.AbstractClassifiers", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.Exams", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.Committees", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.QualificationLevels", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.Rooms", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.Floors", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.Offices", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.Pictures", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.RoomTypes", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.WorkingHours", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.ExternalLinks", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.KudosBaskets", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.Pages", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.AspNetRoles", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.RefreshTokens", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.ServiceRequestComments", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.ServiceRequests", "OrganizationId", c => c.Int(nullable: false));
            AlterColumn("dbo.SyncTokens", "OrganizationId", c => c.Int(nullable: false));
            CreateIndex("dbo.AspNetUsers", "OrganizationId");
            CreateIndex("dbo.AbstractClassifiers", "OrganizationId");
            CreateIndex("dbo.Exams", "OrganizationId");
            CreateIndex("dbo.Committees", "OrganizationId");
            CreateIndex("dbo.QualificationLevels", "OrganizationId");
            CreateIndex("dbo.Rooms", "OrganizationId");
            CreateIndex("dbo.Floors", "OrganizationId");
            CreateIndex("dbo.Offices", "OrganizationId");
            CreateIndex("dbo.Pictures", "OrganizationId");
            CreateIndex("dbo.RoomTypes", "OrganizationId");
            CreateIndex("dbo.WorkingHours", "OrganizationId");
            CreateIndex("dbo.ExternalLinks", "OrganizationId");
            CreateIndex("dbo.KudosBaskets", "OrganizationId");
            CreateIndex("dbo.Pages", "OrganizationId");
            CreateIndex("dbo.AspNetRoles", "OrganizationId");
            CreateIndex("dbo.RefreshTokens", "OrganizationId");
            CreateIndex("dbo.ServiceRequestComments", "OrganizationId");
            CreateIndex("dbo.ServiceRequests", "OrganizationId");
            CreateIndex("dbo.SyncTokens", "OrganizationId");

            CreateIndex("dbo.AspNetRoles", new[] { "OrganizationId", "Name" });
        }

        public override void Down()
        {
            DropIndex("dbo.AspNetRoles", new[] { "OrganizationId", "Name" });
            DropIndex("dbo.SyncTokens", new[] { "OrganizationId" });
            DropIndex("dbo.ServiceRequests", new[] { "OrganizationId" });
            DropIndex("dbo.ServiceRequestComments", new[] { "OrganizationId" });
            DropIndex("dbo.RefreshTokens", new[] { "OrganizationId" });
            DropIndex("dbo.AspNetRoles", new[] { "OrganizationId" });
            DropIndex("dbo.Pages", new[] { "OrganizationId" });
            DropIndex("dbo.KudosBaskets", new[] { "OrganizationId" });
            DropIndex("dbo.ExternalLinks", new[] { "OrganizationId" });
            DropIndex("dbo.WorkingHours", new[] { "OrganizationId" });
            DropIndex("dbo.RoomTypes", new[] { "OrganizationId" });
            DropIndex("dbo.Pictures", new[] { "OrganizationId" });
            DropIndex("dbo.Offices", new[] { "OrganizationId" });
            DropIndex("dbo.Floors", new[] { "OrganizationId" });
            DropIndex("dbo.Rooms", new[] { "OrganizationId" });
            DropIndex("dbo.QualificationLevels", new[] { "OrganizationId" });
            DropIndex("dbo.Committees", new[] { "OrganizationId" });
            DropIndex("dbo.Exams", new[] { "OrganizationId" });
            DropIndex("dbo.AbstractClassifiers", new[] { "OrganizationId" });
            DropIndex("dbo.AspNetUsers", new[] { "OrganizationId" });
            AlterColumn("dbo.SyncTokens", "OrganizationId", c => c.Int());
            AlterColumn("dbo.ServiceRequests", "OrganizationId", c => c.Int());
            AlterColumn("dbo.ServiceRequestComments", "OrganizationId", c => c.Int());
            AlterColumn("dbo.RefreshTokens", "OrganizationId", c => c.Int());
            AlterColumn("dbo.AspNetRoles", "OrganizationId", c => c.Int());
            AlterColumn("dbo.Pages", "OrganizationId", c => c.Int());
            AlterColumn("dbo.KudosBaskets", "OrganizationId", c => c.Int());
            AlterColumn("dbo.ExternalLinks", "OrganizationId", c => c.Int());
            AlterColumn("dbo.WorkingHours", "OrganizationId", c => c.Int());
            AlterColumn("dbo.RoomTypes", "OrganizationId", c => c.Int());
            AlterColumn("dbo.Pictures", "OrganizationId", c => c.Int());
            AlterColumn("dbo.Offices", "OrganizationId", c => c.Int());
            AlterColumn("dbo.Floors", "OrganizationId", c => c.Int());
            AlterColumn("dbo.Rooms", "OrganizationId", c => c.Int());
            AlterColumn("dbo.QualificationLevels", "OrganizationId", c => c.Int());
            AlterColumn("dbo.Committees", "OrganizationId", c => c.Int());
            AlterColumn("dbo.Exams", "OrganizationId", c => c.Int());
            AlterColumn("dbo.AbstractClassifiers", "OrganizationId", c => c.Int());
            AlterColumn("dbo.AspNetUsers", "OrganizationId", c => c.Int());
            CreateIndex("dbo.SyncTokens", "OrganizationId");
            CreateIndex("dbo.ServiceRequests", "OrganizationId");
            CreateIndex("dbo.ServiceRequestComments", "OrganizationId");
            CreateIndex("dbo.RefreshTokens", "OrganizationId");
            CreateIndex("dbo.AspNetRoles", "OrganizationId");
            CreateIndex("dbo.Pages", "OrganizationId");
            CreateIndex("dbo.KudosBaskets", "OrganizationId");
            CreateIndex("dbo.ExternalLinks", "OrganizationId");
            CreateIndex("dbo.WorkingHours", "OrganizationId");
            CreateIndex("dbo.RoomTypes", "OrganizationId");
            CreateIndex("dbo.Pictures", "OrganizationId");
            CreateIndex("dbo.Offices", "OrganizationId");
            CreateIndex("dbo.Floors", "OrganizationId");
            CreateIndex("dbo.Rooms", "OrganizationId");
            CreateIndex("dbo.QualificationLevels", "OrganizationId");
            CreateIndex("dbo.Committees", "OrganizationId");
            CreateIndex("dbo.Exams", "OrganizationId");
            CreateIndex("dbo.AbstractClassifiers", "OrganizationId");
            CreateIndex("dbo.AspNetUsers", "OrganizationId");

            CreateIndex("dbo.AspNetRoles", new[] { "OrganizationId", "Name", "CreatedTime" }, false, "RoleNameIndex2");
        }
    }
}
