using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shrooms.DataLayer.EFCoreMigrations
{
    /// <summary>
    /// Seeds the initial reference data required for a working Simoona installation:
    /// KudosTypes, AspNetRoles, Modules, Organizations, ModuleOrganizations, Permissions,
    /// RolePermissions, ServiceRequestPriorities, ServiceRequestStatus, and a default Wall.
    /// All inserts are guarded with IF NOT EXISTS so the migration is safe to apply to both
    /// fresh installs and existing databases.
    /// </summary>
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- KudosTypes
IF NOT EXISTS (SELECT 1 FROM dbo.KudosTypes)
BEGIN
    INSERT INTO dbo.KudosTypes ([Name], [Value], [IsActive], [Created], [Modified], [IsDeleted], [Description], [Type])
    VALUES
        ('Send',  1, 1, GETUTCDATE(), GETUTCDATE(), 0, 'kudos.typeSend',  2),
        ('Minus', 1, 1, GETUTCDATE(), GETUTCDATE(), 0, 'kudos.typeMinus', 3),
        ('Other', 1, 1, GETUTCDATE(), GETUTCDATE(), 0, 'kudos.typeOther', 4)
END
");

            migrationBuilder.Sql(@"
-- Modules
IF NOT EXISTS (SELECT 1 FROM dbo.Modules)
BEGIN
    SET IDENTITY_INSERT dbo.Modules ON
    INSERT dbo.Modules ([Id], [Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted]) VALUES
        (1, N'Books',          '2018-05-15T11:43:41.810', NULL, '2018-05-15T11:43:41.810', NULL, 0),
        (2, N'ServiceRequest', '2018-05-15T11:43:41.810', NULL, '2018-05-15T11:43:41.810', NULL, 0),
        (3, N'KudosBasket',    '2018-05-15T11:43:41.810', NULL, '2018-05-15T11:43:41.810', NULL, 0),
        (4, N'Monitor',        '2018-05-15T11:43:41.813', NULL, '2018-05-15T11:43:41.813', NULL, 0),
        (5, N'Vacation',       '2018-05-15T11:43:41.813', NULL, '2018-05-15T11:43:41.813', NULL, 0),
        (6, N'Projects',       '2018-05-15T11:43:41.813', NULL, '2018-05-15T11:43:41.813', NULL, 0)
    SET IDENTITY_INSERT dbo.Modules OFF
END
");

            migrationBuilder.Sql(@"
-- Organizations (must come before AspNetRoles and Walls due to FK constraints)
IF NOT EXISTS (SELECT 1 FROM dbo.Organizations WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Organizations ON
    INSERT dbo.Organizations ([Id], [Name], [ShortName], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [HostName], [HasRestrictedAccess], [WelcomeEmail], [RequiresUserConfirmation], [CalendarId], [TimeZone], [BookAppAuthorizationGuid], [CultureCode], [AuthenticationProviders], [KudosYearlyMultipliers])
    VALUES (1, N'testorg', N'testorg', '1900-01-01', NULL, '1900-01-01', NULL, 0, NULL, 0, N'<p style=""text-align:center; font-size:14px; font-weight:400; margin: 0 0 0 0; "">Administrator has confirmed your registration</p>', 0, NULL, N'FLE Standard Time', NULL, N'en-US', N'internal;google;facebook', NULL)
    SET IDENTITY_INSERT dbo.Organizations OFF
END
");

            migrationBuilder.Sql(@"
-- AspNetRoles (FK: OrganizationId → Organizations)
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles)
BEGIN
    INSERT dbo.AspNetRoles ([Id], [Name], [NormalizedName], [OrganizationId], [IsDeleted], [CreatedTime]) VALUES
        (N'2750f71a-056f-4f4a-af9d-6bb524a495c0', N'FirstLogin',                  N'FIRSTLOGIN',                  1, 0, '2018-05-15T11:43:41.610'),
        (N'2c83f542-b496-4ea0-b4d5-5601f303ab28', N'External',                   N'EXTERNAL',                    1, 0, '2018-05-15T11:43:41.610'),
        (N'3b926ec0-88a0-4761-b8c2-869f98f78327', N'User',                       N'USER',                        1, 0, '2018-05-15T11:43:41.610'),
        (N'62c2cde0-1b24-4af8-818c-c6c5ec39ec7a', N'Manager',                    N'MANAGER',                     1, 0, '2018-05-15T11:43:41.610'),
        (N'6c3442bd-b22e-4e57-a316-23753d59c95c', N'Administration',             N'ADMINISTRATION',              1, 0, '2018-05-15T11:43:41.610'),
        (N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a', N'Admin',                      N'ADMIN',                       1, 0, '2018-05-15T11:43:41.610'),
        (N'902462b2-52a4-40d5-8c15-1924681e1c43', N'ServiceRequestNotification', N'SERVICEREQUESTNOTIFICATION',  1, 0, '2018-05-15T11:43:41.610'),
        (N'a0ee55a9-7cf1-4999-adf0-27815725ad24', N'EventsManagement',           N'EVENTSMANAGEMENT',            1, 0, '2018-05-15T11:43:41.610'),
        (N'b20f21fa-1165-419f-8aaf-38bf704f15d1', N'KudosAdmin',                 N'KUDOSADMIN',                  1, 0, '2018-05-15T11:43:41.610'),
        (N'f9301422-9d64-411a-bc6a-7de8325e3b7e', N'ServiceRequest',             N'SERVICEREQUEST',              1, 0, '2018-05-15T11:43:41.610'),
        (N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543', N'NewUser',                    N'NEWUSER',                     1, 0, '2018-05-15T11:43:41.610')
END
");

            migrationBuilder.Sql(@"
-- ModuleOrganization (FK: OrganizationsId → Organizations, ShroomsModulesId → Modules)
IF NOT EXISTS (SELECT 1 FROM dbo.ModuleOrganization WHERE OrganizationsId = 1)
BEGIN
    INSERT dbo.ModuleOrganization ([ShroomsModulesId], [OrganizationsId]) VALUES
        (1, 1), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1)
END
");

            migrationBuilder.Sql(@"
-- Permissions
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions)
BEGIN
    SET IDENTITY_INSERT dbo.Permissions ON
    INSERT dbo.Permissions ([Id], [Name], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Scope], [ModuleId]) VALUES
        (1,  N'MAP_BASIC',                        '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (2,  N'WALL_BASIC',                       '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (3,  N'BOOK_BASIC',                       '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', 1),
        (4,  N'POST_BASIC',                       '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (5,  N'LIKE_BASIC',                       '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (6,  N'KUDOS_BASIC',                      '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (7,  N'COMMENT_BASIC',                    '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (8,  N'BIRTHDAYS_BASIC',                  '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (9,  N'VACATIONS_BASIC',                  '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', 5),
        (10, N'SUPPORT_BASIC',                    '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (11, N'COMMITTEES_BASIC',                 '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (12, N'KUDOSBASKET_BASIC',                '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', 3),
        (13, N'EMPLOYEELIST_BASIC',               '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (14, N'LOCALIZATION_BASIC',               '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (15, N'ORGANIZATION_BASIC',               '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (16, N'EXTERNALLINK_BASIC',               '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (17, N'SERVICEREQUESTS_BASIC',            '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', 2),
        (18, N'ORGANIZATIONALSTRUCTURE_BASIC',    '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (19, N'EXAM_BASIC',                       '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (20, N'ROOM_BASIC',                       '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (21, N'FLOOR_BASIC',                      '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (22, N'SKILL_BASIC',                      '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (23, N'EVENT_BASIC',                      '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (24, N'OFFICE_BASIC',                     '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (25, N'PICTURE_BASIC',                    '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (26, N'CERTIFICATE_BASIC',                '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (27, N'APPLICATIONUSER_BASIC',            '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (28, N'QUALIFICATIONLEVEL_BASIC',         '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', NULL),
        (29, N'PROJECT_BASIC',                    '2018-05-15', NULL, '2018-05-15', NULL, 0, N'basic', 6),
        (30, N'WALL_ADMINISTRATION',              '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (31, N'POST_ADMINISTRATION',              '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (32, N'BOOK_ADMINISTRATION',              '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', 1),
        (33, N'ROLES_ADMINISTRATION',             '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (34, N'ROOM_ADMINISTRATION',              '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (35, N'FLOOR_ADMINISTRATION',             '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (36, N'KUDOS_ADMINISTRATION',             '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (37, N'KUDOSBASKET_ADMINISTRATION',       '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', 3),
        (38, N'OFFICE_ADMINISTRATION',            '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (39, N'PROJECT_ADMINISTRATION',           '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', 6),
        (40, N'ACCOUNT_ADMINISTRATION',           '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (41, N'ROOMTYPE_ADMINISTRATION',          '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (42, N'BIRTHDAYS_ADMINISTRATION',         '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (43, N'CERTIFICATE_ADMINISTRATION',       '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (44, N'EXTERNALLINK_ADMINISTRATION',      '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (45, N'ORGANIZATION_ADMINISTRATION',      '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (46, N'ADMINISTRATION_ADMINISTRATION',    '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (47, N'APPLICATIONUSER_ADMINISTRATION',   '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (48, N'QUALIFICATIONLEVEL_ADMINISTRATION','2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (49, N'EVENT_ADMINISTRATION',             '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (50, N'VACATIONS_ADMINISTRATION',         '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', 5),
        (51, N'COMMITTEES_ADMINISTRATION',        '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (52, N'SERVICEREQUESTS_ADMINISTRATION',   '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', 2),
        (53, N'MONITOR_ADMINISTRATION',           '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', 4),
        (54, N'KUDOSSHOP_ADMINISTRATION',         '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL),
        (55, N'JOB_ADMINISTRATION',               '2018-05-15', NULL, '2018-05-15', NULL, 0, N'admin', NULL)
    SET IDENTITY_INSERT dbo.Permissions OFF
END
");

            migrationBuilder.Sql(@"
-- RolePermissions
IF NOT EXISTS (SELECT 1 FROM dbo.RolePermissions)
BEGIN
    INSERT dbo.RolePermissions ([PermissionsId], [RolesId]) VALUES
        -- External role
        (23, N'2c83f542-b496-4ea0-b4d5-5601f303ab28'),
        -- User role (permissions 1-29)
        (1,  N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (2,  N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (3,  N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (4,  N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (5,  N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (6,  N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (7,  N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (8,  N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (9,  N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (10, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (11, N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (12, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (13, N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (14, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (15, N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (16, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (17, N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (18, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (19, N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (20, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (21, N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (22, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (23, N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (24, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (25, N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (26, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (27, N'3b926ec0-88a0-4761-b8c2-869f98f78327'), (28, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        (29, N'3b926ec0-88a0-4761-b8c2-869f98f78327'),
        -- Administration role (30-35, 38-55)
        (30, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (31, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (32, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (33, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (34, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (35, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (38, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (39, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (40, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (41, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (42, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (43, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (44, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (45, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (46, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (47, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (48, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (49, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (50, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (51, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (53, N'6c3442bd-b22e-4e57-a316-23753d59c95c'), (54, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        (55, N'6c3442bd-b22e-4e57-a316-23753d59c95c'),
        -- Admin role (all 55 permissions)
        (1,  N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (2,  N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (3,  N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (4,  N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (5,  N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (6,  N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (7,  N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (8,  N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (9,  N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (10, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (11, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (12, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (13, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (14, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (15, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (16, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (17, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (18, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (19, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (20, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (21, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (22, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (23, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (24, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (25, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (26, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (27, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (28, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (29, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (30, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (31, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (32, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (33, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (34, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (35, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (36, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (37, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (38, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (39, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (40, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (41, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (42, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (43, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (44, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (45, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (46, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (47, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (48, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (49, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (50, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (51, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (52, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (53, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'), (54, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        (55, N'808fa2cd-28b7-4b8c-beec-433e3ce1a15a'),
        -- ServiceRequestNotification role
        (52, N'902462b2-52a4-40d5-8c15-1924681e1c43'),
        -- EventsManagement role
        (49, N'a0ee55a9-7cf1-4999-adf0-27815725ad24'),
        -- KudosAdmin role
        (36, N'b20f21fa-1165-419f-8aaf-38bf704f15d1'), (37, N'b20f21fa-1165-419f-8aaf-38bf704f15d1'),
        (51, N'b20f21fa-1165-419f-8aaf-38bf704f15d1'),
        -- ServiceRequest role
        (52, N'f9301422-9d64-411a-bc6a-7de8325e3b7e'),
        -- NewUser role
        (19, N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543'), (20, N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543'),
        (21, N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543'), (22, N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543'),
        (24, N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543'), (25, N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543'),
        (26, N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543'), (27, N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543'),
        (28, N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543'), (29, N'fdc6ff2e-8d8e-41e4-a6e7-b844f569b543')
END
");

            migrationBuilder.Sql(@"
-- ServiceRequestPriorities
IF NOT EXISTS (SELECT 1 FROM dbo.ServiceRequestPriorities)
BEGIN
    SET IDENTITY_INSERT dbo.ServiceRequestPriorities ON
    INSERT dbo.ServiceRequestPriorities ([Id], [Title], [IsDeleted], [Created], [CreatedBy], [Modified], [ModifiedBy]) VALUES
        (1, N'Low',   0, '1900-01-01', NULL, '1900-01-01', NULL),
        (2, N'Usual', 0, '1900-01-01', NULL, '1900-01-01', NULL),
        (3, N'High',  0, '1900-01-01', NULL, '1900-01-01', NULL)
    SET IDENTITY_INSERT dbo.ServiceRequestPriorities OFF
END
");

            migrationBuilder.Sql(@"
-- ServiceRequestStatuses
IF NOT EXISTS (SELECT 1 FROM dbo.ServiceRequestStatuses)
BEGIN
    SET IDENTITY_INSERT dbo.ServiceRequestStatuses ON
    INSERT dbo.ServiceRequestStatuses ([Id], [Title], [IsDeleted], [Created], [CreatedBy], [Modified], [ModifiedBy]) VALUES
        (1, N'Open',        0, '1900-01-01', NULL, '1900-01-01', NULL),
        (2, N'In Progress', 0, '1900-01-01', NULL, '1900-01-01', NULL),
        (3, N'Cancelled',   0, '1900-01-01', NULL, '1900-01-01', NULL),
        (4, N'Done',        0, '1900-01-01', NULL, '1900-01-01', NULL),
        (5, N'Purchased',   0, '1900-01-01', NULL, '1900-01-01', NULL)
    SET IDENTITY_INSERT dbo.ServiceRequestStatuses OFF
END
");

            migrationBuilder.Sql(@"
-- Walls
IF NOT EXISTS (SELECT 1 FROM dbo.Walls WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Walls ON
    INSERT dbo.Walls ([Id], [Name], [Description], [OrganizationId], [Created], [CreatedBy], [Modified], [ModifiedBy], [IsDeleted], [Type], [Access], [Logo], [IsHiddenFromAllWalls], [AddForNewUsers])
    VALUES (1, N'Official', N'Official wall', 1, GETDATE(), NULL, GETDATE(), NULL, 0, 0, 0, NULL, 0, 0)
    SET IDENTITY_INSERT dbo.Walls OFF
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Seed data is not rolled back.
        }
    }
}
