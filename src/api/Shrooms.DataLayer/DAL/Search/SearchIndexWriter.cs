using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Shrooms.DataLayer.DAL.Search
{
    public static class SearchIndexWriter
    {
        private const string Columns =
            "OrganizationId, Kind, EntityId, Title, Description, Date, PictureId, LinkId, AccessKey, OwnerUserId, SearchText, IndexedAt";

        /// <summary>Re-reads the given source rows and inserts, updates or removes their search rows.</summary>
        public static async Task RefreshAsync(DbContext context, SearchIndexSource source, IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
        {
            var parameter = new SqlParameter("@keys", JsonSerializer.Serialize(keys));
            await context.Database.ExecuteSqlRawAsync(BuildRefreshSql(source), new object[] { parameter }, cancellationToken);
        }

        public static void Refresh(DbContext context, SearchIndexSource source, IReadOnlyCollection<string> keys)
        {
            var parameter = new SqlParameter("@keys", JsonSerializer.Serialize(keys));
            context.Database.ExecuteSqlRaw(BuildRefreshSql(source), parameter);
        }

        /// <summary>Makes the search rows of one source match the source table exactly.</summary>
        public static Task<int> RebuildAsync(DbContext context, SearchIndexSource source, CancellationToken cancellationToken = default)
        {
            return context.Database.ExecuteSqlRawAsync(BuildMergeSql(source, byKeys: false), cancellationToken);
        }

        internal static string BuildRefreshSql(SearchIndexSource source)
        {
            var sql = BuildMergeSql(source, byKeys: true);
            if (!source.FilterKeyIsEntityId)
            {
                return sql;
            }

            // A row removed outright (not soft-deleted) no longer shows up in the source.
            return sql + $@"
DELETE d FROM dbo.SearchDocuments d
WHERE d.Kind = {(byte)source.Kind}
  AND d.EntityId IN (SELECT [value] FROM OPENJSON(@keys))
  AND NOT EXISTS (SELECT 1 FROM ({source.Sql}) s WHERE s.EntityId = d.EntityId AND s.OrganizationId = d.OrganizationId);";
        }

        // Only changed rows are written, so a nightly rebuild leaves the full-text index
        // alone. Binary collation, so a case- or accent-only edit still counts as a change.
        private static string Compared(string alias) =>
            string.Join(", ", new[] { "Title", "Description", "PictureId", "LinkId", "AccessKey", "OwnerUserId", "SearchText" }
                .Select(column => $"{alias}.{column} COLLATE Latin1_General_100_BIN2")
                .Append($"{alias}.Date"));

        internal static string BuildMergeSql(SearchIndexSource source, bool byKeys)
        {
            var kind = (byte)source.Kind;
            var filter = byKeys ? "WHERE s.FilterKey IN (SELECT [value] FROM OPENJSON(@keys))" : string.Empty;
            var removeMissing = byKeys ? string.Empty : "WHEN NOT MATCHED BY SOURCE THEN DELETE";

            return $@"
WITH target AS (SELECT * FROM dbo.SearchDocuments WHERE Kind = {kind})
MERGE target WITH (HOLDLOCK) AS t
USING (
    SELECT s.* FROM ({source.Sql}) s
    {filter}
) AS s
ON t.OrganizationId = s.OrganizationId AND t.EntityId = s.EntityId
WHEN MATCHED AND s.IsSearchable = 0 THEN DELETE
WHEN MATCHED AND EXISTS (
    SELECT {Compared("s")}
    EXCEPT
    SELECT {Compared("t")}) THEN UPDATE SET
    Title = s.Title,
    Description = s.Description,
    Date = s.Date,
    PictureId = s.PictureId,
    LinkId = s.LinkId,
    AccessKey = s.AccessKey,
    OwnerUserId = s.OwnerUserId,
    SearchText = s.SearchText,
    IndexedAt = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET AND s.IsSearchable = 1 THEN
    INSERT ({Columns})
    VALUES (s.OrganizationId, {kind}, s.EntityId, s.Title, s.Description, s.Date, s.PictureId, s.LinkId, s.AccessKey, s.OwnerUserId, s.SearchText, SYSUTCDATETIME())
{removeMissing};";
        }
    }
}
