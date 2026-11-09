using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.DAL.Search
{
    public sealed class SearchIndexQuery
    {
        public int OrganizationId { get; init; }

        public string UserId { get; init; }

        /// <summary>Words of letters and digits only, folded by <see cref="SearchTextNormalizer"/>.</summary>
        public IReadOnlyList<string> Terms { get; init; }

        public IReadOnlyCollection<SearchDocumentKind> Kinds { get; init; }

        /// <summary>Kinds whose owner and access-key limits the user is exempt from (admins).</summary>
        public IReadOnlyCollection<SearchDocumentKind> UnrestrictedKinds { get; init; }

        public bool CanSeeEventWalls { get; init; }

        public int LimitPerKind { get; init; }
    }

    public sealed class SearchIndexHit
    {
        public byte Kind { get; set; }

        public string EntityId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? Date { get; set; }

        public string PictureId { get; set; }

        public string LinkId { get; set; }

        public int Total { get; set; }
    }

    public static class SearchIndexReader
    {
        private static readonly TimeSpan FullTextCheckInterval = TimeSpan.FromMinutes(5);
        private static readonly ConcurrentDictionary<string, (bool Available, DateTime CheckedAt)> FullTextByDatabase = new();

        /// <summary>
        /// Top matches per kind with each kind's total, in one round trip. Uses the
        /// full-text index when the database has one, else LIKE over SearchText.
        /// </summary>
        public static async Task<IReadOnlyList<SearchIndexHit>> SearchAsync(DbContext context, SearchIndexQuery query, CancellationToken cancellationToken = default)
        {
            if (query.Terms.Count == 0 || query.Kinds.Count == 0)
            {
                return Array.Empty<SearchIndexHit>();
            }

            var useFullText = await HasFullTextIndexAsync(context, cancellationToken);
            var parameters = new object[]
            {
                new SqlParameter("@organizationId", query.OrganizationId),
                new SqlParameter("@userId", query.UserId),
                new SqlParameter("@kinds", JsonSerializer.Serialize(query.Kinds.Select(kind => (byte)kind))),
                new SqlParameter("@unrestrictedKinds", JsonSerializer.Serialize((query.UnrestrictedKinds ?? Array.Empty<SearchDocumentKind>()).Select(kind => (byte)kind))),
                new SqlParameter("@canSeeEventWalls", query.CanSeeEventWalls),
                new SqlParameter("@limit", query.LimitPerKind),
                new SqlParameter("@terms", JsonSerializer.Serialize(query.Terms)),
                new SqlParameter("@firstTerm", query.Terms[0]) { SqlDbType = System.Data.SqlDbType.NVarChar, Size = 40 },
                new SqlParameter("@fullTextQuery", string.Join(" AND ", query.Terms.Select(term => $"\"{term}*\"")))
            };

            try
            {
                return await context.Database
                    .SqlQueryRaw<SearchIndexHit>(BuildSearchSql(useFullText), parameters)
                    .ToListAsync(cancellationToken);
            }
            catch (SqlException) when (cancellationToken.IsCancellationRequested)
            {
                // SqlClient reports a cancelled raw query as a SqlException; rethrown as a
                // cancellation, RequestAbortedExceptionFilter answers 499 instead of logging it.
                throw new OperationCanceledException(cancellationToken);
            }
        }

        internal static string BuildSearchSql(bool useFullText)
        {
            // SearchText starts with the title in every source, so its folded opening stands
            // in for the title when ranking, without an accent-insensitive comparison per row.
            var matches = useFullText
                ? $@"SELECT d.*, LEFT(d.SearchText, 200) AS Lead, ft.[RANK] AS MatchScore
    FROM CONTAINSTABLE(dbo.SearchDocuments, SearchText, @fullTextQuery) ft
    JOIN dbo.SearchDocuments d ON d.Id = ft.[KEY]"
                : $@"SELECT d.*, LEFT(d.SearchText, 200) AS Lead, 0 AS MatchScore
    FROM dbo.SearchDocuments d
    WHERE NOT EXISTS (
        SELECT 1 FROM OPENJSON(@terms) t
        WHERE d.SearchText NOT LIKE N'%' + t.[value] COLLATE {SearchTextNormalizer.Collation} + N'%')";

            // The user's access keys and the kind lists are worked out once up front: as
            // subqueries in the WHERE they were re-evaluated for every candidate row.
            return $@"
SET NOCOUNT ON;
DECLARE @access TABLE (AccessKey nvarchar(200) COLLATE DATABASE_DEFAULT PRIMARY KEY);
INSERT INTO @access (AccessKey)
SELECT CAST(CONCAT(N'wall:', wm.WallId) AS nvarchar(200))
FROM dbo.WallMembers wm
JOIN dbo.Walls w ON w.Id = wm.WallId
WHERE @canSeeEventWalls = 1 AND wm.UserId = @userId AND wm.IsDeleted = 0 AND w.IsDeleted = 0 AND w.Type = 2
UNION
SELECT CAST(CONCAT(N'category:', c.Name COLLATE DATABASE_DEFAULT) AS nvarchar(200))
FROM dbo.ServiceRequestCategoryApplicationUsers cu
JOIN dbo.ServiceRequestCategories c ON c.Id = cu.ServiceRequestCategory_Id
WHERE cu.ApplicationUser_Id = @userId;

DECLARE @kindList TABLE (Kind tinyint PRIMARY KEY);
INSERT INTO @kindList (Kind) SELECT DISTINCT CAST([value] AS tinyint) FROM OPENJSON(@kinds);

DECLARE @unrestrictedList TABLE (Kind tinyint PRIMARY KEY);
INSERT INTO @unrestrictedList (Kind) SELECT DISTINCT CAST([value] AS tinyint) FROM OPENJSON(@unrestrictedKinds);

WITH matches AS (
    {matches}
),
hits AS (
    SELECT m.Kind, m.EntityId, m.Title, m.Description, m.Date, m.PictureId, m.LinkId,
           m.MatchScore
           + CASE WHEN m.Lead LIKE @firstTerm + N'%' THEN 500
                  WHEN m.Lead LIKE N'% ' + @firstTerm + N'%' THEN 300
                  WHEN m.Lead LIKE N'%' + @firstTerm + N'%' THEN 100
                  ELSE 0 END AS Score
    FROM matches m
    JOIN @kindList k ON k.Kind = m.Kind
    LEFT JOIN @unrestrictedList u ON u.Kind = m.Kind
    LEFT JOIN @access a ON a.AccessKey = m.AccessKey
    WHERE m.OrganizationId = @organizationId
      AND ((m.AccessKey IS NULL AND m.OwnerUserId IS NULL)
           OR m.OwnerUserId = @userId
           OR u.Kind IS NOT NULL
           OR a.AccessKey IS NOT NULL)
),
ranked AS (
    SELECT *,
           ROW_NUMBER() OVER (PARTITION BY Kind ORDER BY Score DESC, Date DESC, EntityId) AS RowNumber,
           COUNT(*) OVER (PARTITION BY Kind) AS Total
    FROM hits
)
SELECT Kind, EntityId, Title, Description, Date, PictureId, LinkId, Total
FROM ranked
WHERE RowNumber <= @limit
ORDER BY Kind, RowNumber";
        }

        private static async Task<bool> HasFullTextIndexAsync(DbContext context, CancellationToken cancellationToken)
        {
            var connection = context.Database.GetDbConnection();
            var key = $"{connection.DataSource}/{connection.Database}";
            if (FullTextByDatabase.TryGetValue(key, out var cached) && DateTime.UtcNow - cached.CheckedAt < FullTextCheckInterval)
            {
                return cached.Available;
            }

            var available = await context.Database
                .SqlQueryRaw<bool>("SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.SearchDocuments')) THEN 1 ELSE 0 END AS bit) AS [Value]")
                .SingleAsync(cancellationToken);

            FullTextByDatabase[key] = (available, DateTime.UtcNow);
            return available;
        }
    }
}
