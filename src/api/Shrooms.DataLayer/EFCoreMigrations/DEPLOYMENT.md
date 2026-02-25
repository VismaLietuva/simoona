# EF Core Migration Deployment Guide

Migrations are **not applied automatically at runtime**. They must be applied as an explicit deployment step before the application starts. The app will throw an `InvalidOperationException` on startup if any pending migrations are detected.

---

## Applying migrations

### Option A — CLI (recommended for CI/CD)

```powershell
cd src/api

# Update the database directly
dotnet ef database update \
  --project Shrooms.DataLayer \
  --connection "Server=...;Database=SimoonaDB;..."
```

### Option B — Generate idempotent SQL script (preferred for production/DBA review)

```powershell
cd src/api

dotnet ef migrations script \
  --idempotent \
  --project Shrooms.DataLayer \
  --output migration.sql
```

The `--idempotent` flag makes the script check `__EFMigrationsHistory` before applying each migration, so it is safe to run multiple times and on any database state.

---

## First deployment to an existing EF6 database (brownfield)

Before running `dotnet ef database update` (or the generated script) for the very first time on a production database that was previously managed by EF6, you must run this one-time bootstrap SQL:

```sql
-- 1. Create the EF Core migrations history table
CREATE TABLE [__EFMigrationsHistory] (
    [MigrationId] nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);

-- 2. Mark InitialBaseline as already applied
--    (the existing EF6 schema already has all those tables — no need to recreate them)
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20260225115301_InitialBaseline', '10.0.0');
```

After running the above, execute `dotnet ef database update` (or the idempotent SQL script) normally. Only the migrations after `InitialBaseline` will be applied — for example `AddIdentityV3Columns`.

---

## Adding new migrations

```powershell
cd src/api
dotnet ef migrations add <MigrationName> \
  --project Shrooms.DataLayer \
  --output-dir EFCoreMigrations

# Verify the snapshot matches the model after adding
dotnet ef migrations has-pending-model-changes --project Shrooms.DataLayer
```
