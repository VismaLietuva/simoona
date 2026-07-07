# Docker Setup Guide

This guide explains how to get Simoona running from a clean repository checkout using Docker.

## Prerequisites

| Tool | Purpose |
|------|---------|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Runs all containers |

| .NET 10 SDK | Only needed for [local fast-mode development](#local-development-fast-mode) |
| Node.js + gulp | Only needed for [local fast-mode development](#local-development-fast-mode) |

---

## 1. Start the stack

```bash
cd src/api
docker compose up -d
```

This starts three containers:

| Container | Description |
|-----------|-------------|
| `simoona-db` | SQL Server 2022. Creates `SimoonaDB` and `SimoonaDBJobs` on first boot. |
| `simoona-api` | .NET 10 API. Waits for the DB to be healthy, then applies all EF Core migrations automatically on startup. |
| `simoona-webapp` | AngularJS frontend served by Node. |

Wait for the API to finish migrating before proceeding:

```bash
docker logs simoona-api --follow
```

Ready when you see: `Server ... all the dispatchers started`

---

## 2. Seed reference data and create the first admin user

Run the setup script once against the database. It seeds all reference data (roles, permissions, modules, etc.) and creates your first admin account.

**Windows (PowerShell):**

```powershell
.\build\setup.ps1 `
  -ConnectionString "Server=localhost,1434;Database=SimoonaDB;User Id=sa;Password=Password!123;TrustServerCertificate=True" `
  -Email tester@example.com `
  -Password 'testerPass123' `
  -OrgName testorg
```

**Linux / macOS:**

```bash
./build/setup.sh \
  "Server=localhost,1434;Database=SimoonaDB;User Id=sa;Password=Password!123;TrustServerCertificate=True" \
  tester@example.com 'testerPass123' testorg
```

The `-OrgName` / fourth argument is optional (defaults to `testorg`).
The script is **idempotent** — safe to run again after a partial failure.

---

## 3. Open the app

| Service | URL |
|---------|-----|
| Web app | http://localhost:3000 |
| API | http://localhost:50321 |
| Swagger UI | http://localhost:50321/swagger |
| Hangfire dashboard | http://localhost:50321/hangfire |

Log in with `tester@example.com` / `testerPass123` (or whatever you passed to the setup script).

---

## Local development (fast mode)

Fast mode mounts your local build output into the containers instead of rebuilding Docker images on every change.

### One-time setup

**Build the API:**
```powershell
dotnet build src/api/Shrooms.Presentation.Api -c Debug
```

**Build the webapp** (requires Node.js and gulp):
```bash
cd src/webapp
npm install
gulp build-dev
```

### Start with the override file

```bash
cd src/api
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

### Iterating

After changing API code, rebuild on the host and restart the container:
```powershell
dotnet build src/api/Shrooms.Presentation.Api -c Debug
docker restart simoona-api
```

After changing webapp code:
```bash
cd src/webapp && gulp build-dev
docker restart simoona-webapp
```

---

## Stopping and resetting

```bash
# Stop all containers (keeps database data)
docker compose down

# Full reset — removes containers AND the database volume
docker compose down -v
```

After a full reset (`-v`), repeat steps 1–2.

---

## What each build script does

| File | Purpose |
|------|---------|
| `build/seed.sql` | Idempotent SQL that inserts all reference data (roles, permissions, modules, walls, etc.). Run by `setup.ps1/sh`. |
| `build/setup.ps1` | Windows: seeds the DB and creates the first admin user using System.Data.SqlClient (no external tools required). |
| `build/setup.sh` | Linux/macOS equivalent of `setup.ps1`. Requires Python 3 for password hashing. |
| `src/api/docker-compose.yml` | Production-style Docker Compose (images built from source). |
| `src/api/docker-compose.override.yml` | Local dev override (bind-mounts host build output). |
| `src/api/db/init-db.sh` | Entrypoint for the `simoona-db` container — creates both SQL Server databases on first run. |
