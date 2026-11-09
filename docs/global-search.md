# Global search

The search overlay in the Next.js app calls `GET /Search?query=&context=`. It reads a single table, `SearchDocuments`, with one row per searchable item. Each search is one SQL round trip: the top results per section plus each section's total.

## How it works

| Part | Where |
| --- | --- |
| Table and full-text index | migration `AddGlobalSearch` |
| What each section contributes, and who may find it | `Shrooms.DataLayer/DAL/Search/SearchIndexSources.cs` |
| Writing rows (refresh by key, full rebuild) | `SearchIndexWriter.cs` |
| Reading (full-text, or LIKE when the database has no full-text) | `SearchIndexReader.cs` |
| Keeping rows current on save | `SearchIndexChangeSet.cs`, called from every `ShroomsDbContext.SaveChanges*` |
| Permissions per section, endpoint | `SearchService.cs`, `SearchController.cs` |
| Nightly repair | `POST /ExternalJobs/RebuildSearchIndex` |

**Keeping rows current**
- Every save that touches a searchable entity refreshes that entity's row right after the save commits.
- The refresh is best effort. If it fails, the failure is logged as a warning and the save still succeeds.
- It is skipped inside an explicit transaction.
- Some changes happen outside `SaveChanges`: bulk `ExecuteUpdate`, raw SQL, or a renamed user whose name also appears in kudos rows. The nightly rebuild picks these up.

**Storage built for speed**
- `SearchText` is stored lower-case with accents removed ("Žukauskas" → "zukauskas") by `SearchTextNormalizer`. Typed words are cleaned the same way.
  - It is compared with a binary collation, which is several times faster than an accent-insensitive comparison.
  - It is capped at 4,000 characters, so it stays in-row.
- The table is clustered on `(OrganizationId, Kind, EntityId)`, so one organisation's sections are read in one pass. `Id` is the non-clustered key that the full-text index requires.
- Every text column in the source queries is collated to the database default explicitly. Production databases mix collations (user names are `Lithuanian_CI_AI`).

**Full-text search**
- **Azure SQL** has full-text search, so the migration creates the `SearchCatalog` catalog and a full-text index. The catalog is accent-insensitive and has no stoplist.
- **The local Docker image** has no full-text search. There the table is created without the index, and search falls back to `LIKE` with an accent-insensitive collation. The reader checks every five minutes which mode the database supports.

## Rollout

1. **Deploy.** Migrations run on startup for every tenant and create the empty table. Nothing reads it yet, and saves start filling it.
2. **Fill the table.** Call `POST /ExternalJobs/RebuildSearchIndex` once per tenant, with the same Basic auth and `Organization` header as the other external jobs. It is safe to run again: unchanged rows are not rewritten.
3. **Schedule the rebuild nightly** for every tenant, next to the other external jobs.
4. **Turn it on.** Create the `GlobalSearch` feature flag in Administration → Feature flags.
   - Enable it for a few users first, then for the whole organisation.
   - Without the flag, the search button is hidden and `/Search` answers 404.

## Rollback

Use the lightest step that solves the problem.

1. **Hide search.** Turn the `GlobalSearch` flag off (or remove its users). This takes effect immediately and needs no deploy.
2. **Stop writes on save.** Set `GlobalSearch__IndexOnSave=false` in the App Service configuration; the app restarts.
   - Saves no longer touch `SearchDocuments`.
   - Use this if the extra statement after a save causes trouble.
   - Stop the nightly rebuild as well.
3. **Remove the code.** Revert the commit and deploy.
   - The table stays behind, unused and harmless.
   - The previous code ignores the newer migration in `__EFMigrationsHistory`.
4. **Remove the table.** Run the migration's `Down` against each tenant database:
   ```bash
   dotnet ef migrations script AddGlobalSearch AddTimeRegistrationOvertime --project src/api/Shrooms.DataLayer --startup-project src/api/Shrooms.DataLayer -o remove-global-search.sql
   ```
   Run the script against each tenant. It drops the full-text index, the catalog and the table, and removes the history row.
   - Do step 2 or 3 first. Otherwise saves log a warning every time they try to write to the missing table.
