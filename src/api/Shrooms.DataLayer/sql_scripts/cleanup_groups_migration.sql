/*
    Removes the Groups feature from a database so the AddGroups migration can be
    applied again from scratch.

    Why this is needed
    ------------------
    The AddGroups migration was regenerated several times during development, so its
    migration id changed. A database that was deployed with an earlier id keeps that
    id in __EFMigrationsHistory. On the next start, Program.cs runs
    db.Database.Migrate() per tenant; EF does not recognise the stored id, tries to
    apply the current AddGroups, and fails because the tables already exist. The
    container then dies during startup (exit 134).

    This script drops every table the feature has ever created, removes its
    permissions, and deletes the history row. The next deployment applies the
    current AddGroups cleanly.

    IMPORTANT
    ---------
    Simoona is multi-tenant: Program.cs migrates every tenant connection string it
    is configured with. Run this script against EVERY tenant database on the
    environment, not just one, or the next start will fail on whichever database
    was missed.

    Safe to run more than once, and safe on a database that never received Groups -
    every step is guarded.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

------------------------------------------------------------------------------
-- 1. Drop the tables, children before parents.
--    The list covers every shape the migration went through, so it works
--    whichever version this database was deployed with.
------------------------------------------------------------------------------
DROP TABLE IF EXISTS dbo.GroupSuggestionsIDs;      -- join, early versions only
DROP TABLE IF EXISTS dbo.GroupDelegates;           -- join, early versions only
DROP TABLE IF EXISTS dbo.GroupMembers;
DROP TABLE IF EXISTS dbo.GroupReferences;
DROP TABLE IF EXISTS dbo.GroupMonthlyKudosAwards;  -- early versions only
DROP TABLE IF EXISTS dbo.GroupSuggestions;         -- early versions only
DROP TABLE IF EXISTS dbo.Groups;
DROP TABLE IF EXISTS dbo.GroupTypes;

------------------------------------------------------------------------------
-- 2. Remove the permissions the migration seeded, and any role grants.
------------------------------------------------------------------------------
DECLARE @permissionIds TABLE (Id INT);

INSERT INTO @permissionIds (Id)
SELECT Id
FROM   dbo.Permissions
WHERE  Name IN (N'GROUPS_BASIC', N'GROUPS_ADMINISTRATION');

DELETE FROM dbo.RolePermissions WHERE PermissionId IN (SELECT Id FROM @permissionIds);
DELETE FROM dbo.Permissions     WHERE Id           IN (SELECT Id FROM @permissionIds);

------------------------------------------------------------------------------
-- 3. Forget the migration, whichever id this database recorded.
------------------------------------------------------------------------------
DECLARE @removedHistory TABLE (MigrationId NVARCHAR(150));

DELETE FROM dbo.__EFMigrationsHistory
OUTPUT deleted.MigrationId INTO @removedHistory
WHERE MigrationId LIKE N'%_AddGroups';

COMMIT TRANSACTION;

------------------------------------------------------------------------------
-- 4. Report what happened, so a run against the wrong database is obvious.
------------------------------------------------------------------------------
SELECT DB_NAME()                                            AS [Database],
       (SELECT COUNT(*) FROM sys.tables
        WHERE name IN (N'GroupTypes', N'Groups', N'GroupMembers', N'GroupReferences',
                       N'GroupSuggestions', N'GroupSuggestionsIDs', N'GroupDelegates',
                       N'GroupMonthlyKudosAwards'))         AS [GroupTablesRemaining],
       (SELECT COUNT(*) FROM dbo.Permissions
        WHERE Name LIKE N'GROUPS[_]%')                      AS [GroupPermissionsRemaining],
       (SELECT COUNT(*) FROM @removedHistory)               AS [HistoryRowsRemoved];

SELECT MigrationId AS [RemovedMigration] FROM @removedHistory;
