-- Baselines the EF Core migration history for databases originally created by the
-- .NET Framework app (which had no __EFMigrationsHistory table).
--
-- Run this ONCE on each brownfield database BEFORE the first .NET 10 deployment.
-- After this script runs, the app's Migrate() call will skip InitialBaseline (tables
-- already exist) and apply only the subsequent migrations (AddIdentityV3Columns, etc.)
-- which add the columns/tables missing from the old Identity v2 schema.
--
-- Safe to run multiple times — all statements are idempotent.

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE __EFMigrationsHistory (
        MigrationId     NVARCHAR(150) NOT NULL,
        ProductVersion  NVARCHAR(32)  NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
    );
END

IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260225115301_InitialBaseline')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260225115301_InitialBaseline', '10.0.0');
END
