using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.DAL.Search
{
    /// <summary>
    /// What each section contributes to global search, and who may find it. The
    /// visibility rules mirror the section's own list queries; anything finer than
    /// "who may find this row" (e.g. wall membership) is resolved at query time
    /// through AccessKey and OwnerUserId.
    /// </summary>
    public static class SearchIndexSources
    {
        private const string Flatten = "REPLACE(REPLACE({0}, CHAR(13), N' '), CHAR(10), N' ')";

        // Explicit collation throughout: production databases mix column collations (names
        // are Lithuanian_CI_AI there), and CONCAT refuses to combine two implicit ones.
        private static string FullName(string alias) =>
            $"CONCAT_WS(N' ', {alias}.FirstName COLLATE DATABASE_DEFAULT, {alias}.LastName COLLATE DATABASE_DEFAULT)";

        // Main (0) and user-created (1) walls are open to everyone with POST_BASIC; posts on
        // event walls (2) only to the event's wall members, hence the access key. Project and
        // poll walls never show up in the feed or search, so neither do their posts.
        private static string PostsSql(string filterKey) => $@"
SELECT {filterKey} AS FilterKey,
       w.OrganizationId,
       CAST(p.Id AS nvarchar(128)) AS EntityId,
       LEFT(LTRIM({string.Format(Flatten, "ISNULL(p.MessageBody, N'')")}), 400) AS Title,
       LEFT(CONCAT_WS(N' · ', {FullName("a")}, w.Name COLLATE DATABASE_DEFAULT), 400) AS Description,
       p.Created AS Date,
       CAST(CASE WHEN ISJSON(p.Images) = 1 THEN JSON_VALUE(p.Images, '$[0]') END AS nvarchar(128)) AS PictureId,
       CAST(NULL AS nvarchar(128)) AS LinkId,
       CAST(CASE WHEN w.Type = 2 THEN CONCAT(N'wall:', w.Id) END AS nvarchar(200)) AS AccessKey,
       CAST(NULL AS nvarchar(128)) AS OwnerUserId,
       {SearchTextNormalizer.Sql("ISNULL(p.MessageBody, N'')")} AS SearchText,
       CASE WHEN p.IsDeleted = 0 AND p.IsHidden = 0 AND w.IsDeleted = 0 AND w.Type IN (0, 1, 2)
                 AND LEN(ISNULL(p.MessageBody, N'')) > 0 THEN 1 ELSE 0 END AS IsSearchable
FROM dbo.Posts p
JOIN dbo.Walls w ON w.Id = p.WallId
LEFT JOIN dbo.AspNetUsers a ON a.Id = p.AuthorId";

        public static readonly SearchIndexSource Posts = new("Posts", SearchDocumentKind.Post, PostsSql("p.Id"));

        public static readonly SearchIndexSource PostsByWall = new("PostsByWall", SearchDocumentKind.Post, PostsSql("p.WallId"), filterKeyIsEntityId: false);

        // New users waiting for approval are left out, as on the employee list.
        public static readonly SearchIndexSource People = new("People", SearchDocumentKind.Person, $@"
SELECT u.Id AS FilterKey,
       u.OrganizationId,
       u.Id AS EntityId,
       LEFT({FullName("u")}, 400) AS Title,
       LEFT(j.Title, 400) AS Description,
       CAST(NULL AS datetime2) AS Date,
       CAST(u.PictureId AS nvarchar(128)) AS PictureId,
       CAST(NULL AS nvarchar(128)) AS LinkId,
       CAST(NULL AS nvarchar(200)) AS AccessKey,
       CAST(NULL AS nvarchar(128)) AS OwnerUserId,
       {SearchTextNormalizer.Sql($"CONCAT_WS(N' ', {FullName("u")}, j.Title COLLATE DATABASE_DEFAULT, u.Email COLLATE DATABASE_DEFAULT)")} AS SearchText,
       CASE WHEN u.IsDeleted = 0 AND ISNULL(u.IsAnonymized, 0) = 0
                 AND LEN({FullName("u")}) > 0
                 AND NOT EXISTS (
                     SELECT 1 FROM dbo.AspNetUserRoles ur
                     JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
                     WHERE ur.UserId = u.Id AND r.Name = N'NewUser')
            THEN 1 ELSE 0 END AS IsSearchable
FROM dbo.AspNetUsers u
LEFT JOIN dbo.JobPositions j ON j.Id = u.JobPositionId AND j.IsDeleted = 0");

        public static readonly SearchIndexSource Events = new("Events", SearchDocumentKind.Event, $@"
SELECT e.Id AS FilterKey,
       ISNULL(e.OrganizationId, 0) AS OrganizationId,
       CAST(e.Id AS nvarchar(128)) AS EntityId,
       LEFT(e.Name, 400) AS Title,
       LEFT(e.Place, 400) AS Description,
       CAST(e.StartDate AS datetime2) AS Date,
       CAST(e.ImageName AS nvarchar(128)) AS PictureId,
       CAST(NULL AS nvarchar(128)) AS LinkId,
       CAST(NULL AS nvarchar(200)) AS AccessKey,
       CAST(NULL AS nvarchar(128)) AS OwnerUserId,
       {SearchTextNormalizer.Sql("CONCAT_WS(N' ', e.Name COLLATE DATABASE_DEFAULT, e.Place COLLATE DATABASE_DEFAULT, LEFT(e.Description, 2000) COLLATE DATABASE_DEFAULT)")} AS SearchText,
       CASE WHEN e.IsDeleted = 0 AND e.OrganizationId IS NOT NULL AND LEN(ISNULL(e.Name, N'')) > 0 THEN 1 ELSE 0 END AS IsSearchable
FROM dbo.Events e");

        // As on the kudos wall widget: approved only, no minus or refund entries, no basket
        // donations, and a comment the receiver chose to hide is neither shown nor matched.
        public static readonly SearchIndexSource Kudos = new("Kudos", SearchDocumentKind.Kudos, $@"
SELECT k.Id AS FilterKey,
       k.OrganizationId,
       CAST(k.Id AS nvarchar(128)) AS EntityId,
       LEFT(CONCAT({FullName("s")}, N' → ', {FullName("r")}), 400) AS Title,
       LEFT(CASE WHEN ISNULL(k.IsCommentHidden, 0) = 1 THEN k.KudosTypeName
                 ELSE CONCAT_WS(N' · ', k.KudosTypeName COLLATE DATABASE_DEFAULT, {string.Format(Flatten, "k.Comments COLLATE DATABASE_DEFAULT")}) END, 400) AS Description,
       CAST(k.Created AS datetime2) AS Date,
       CAST(NULL AS nvarchar(128)) AS PictureId,
       CAST(k.EmployeeId AS nvarchar(128)) AS LinkId,
       CAST(NULL AS nvarchar(200)) AS AccessKey,
       CAST(NULL AS nvarchar(128)) AS OwnerUserId,
       {SearchTextNormalizer.Sql($"CONCAT_WS(N' ', {FullName("s")}, {FullName("r")}, k.KudosTypeName COLLATE DATABASE_DEFAULT, CASE WHEN ISNULL(k.IsCommentHidden, 0) = 0 THEN k.Comments COLLATE DATABASE_DEFAULT END)")} AS SearchText,
       CASE WHEN k.Status = 2 AND k.KudosSystemType NOT IN (3, 6) AND k.KudosBasketId IS NULL THEN 1 ELSE 0 END AS IsSearchable
FROM dbo.KudosLogs k
LEFT JOIN dbo.AspNetUsers s ON s.Id = k.CreatedBy
LEFT JOIN dbo.AspNetUsers r ON r.Id = k.EmployeeId");

        public static readonly SearchIndexSource Projects = new("Projects", SearchDocumentKind.Project, $@"
SELECT p.Id AS FilterKey,
       p.OrganizationId,
       CAST(p.Id AS nvarchar(128)) AS EntityId,
       LEFT(p.Name, 400) AS Title,
       LEFT(p.[Desc], 400) AS Description,
       CAST(NULL AS datetime2) AS Date,
       CAST(p.Logo AS nvarchar(128)) AS PictureId,
       CAST(NULL AS nvarchar(128)) AS LinkId,
       CAST(NULL AS nvarchar(200)) AS AccessKey,
       CAST(NULL AS nvarchar(128)) AS OwnerUserId,
       {SearchTextNormalizer.Sql("CONCAT_WS(N' ', p.Name COLLATE DATABASE_DEFAULT, LEFT(p.[Desc], 2000) COLLATE DATABASE_DEFAULT)")} AS SearchText,
       CASE WHEN p.IsDeleted = 0 AND LEN(ISNULL(p.Name, N'')) > 0 THEN 1 ELSE 0 END AS IsSearchable
FROM dbo.Projects p");

        // Pending groups are visible only to admins and members, so search leaves them out.
        public static readonly SearchIndexSource Groups = new("Groups", SearchDocumentKind.Group, $@"
SELECT g.Id AS FilterKey,
       g.OrganizationId,
       CAST(g.Id AS nvarchar(128)) AS EntityId,
       LEFT(g.Name, 400) AS Title,
       LEFT(g.Description, 400) AS Description,
       CAST(NULL AS datetime2) AS Date,
       CAST(g.PictureId AS nvarchar(128)) AS PictureId,
       CAST(NULL AS nvarchar(128)) AS LinkId,
       CAST(NULL AS nvarchar(200)) AS AccessKey,
       CAST(NULL AS nvarchar(128)) AS OwnerUserId,
       {SearchTextNormalizer.Sql("CONCAT_WS(N' ', g.Name COLLATE DATABASE_DEFAULT, LEFT(g.Description, 2000) COLLATE DATABASE_DEFAULT)")} AS SearchText,
       CASE WHEN g.IsDeleted = 0 AND g.Status = 1 AND LEN(ISNULL(g.Name, N'')) > 0 THEN 1 ELSE 0 END AS IsSearchable
FROM dbo.Groups g");

        // The book page is per office copy (BookOffices), so each copy is its own result.
        private static string BooksSql(string filterKey) => $@"
SELECT {filterKey} AS FilterKey,
       bo.OrganizationId,
       CAST(bo.Id AS nvarchar(128)) AS EntityId,
       LEFT(b.Title, 400) AS Title,
       LEFT(CONCAT_WS(N' · ', b.Author COLLATE DATABASE_DEFAULT, o.Name COLLATE DATABASE_DEFAULT), 400) AS Description,
       CAST(NULL AS datetime2) AS Date,
       CAST(NULL AS nvarchar(128)) AS PictureId,
       CAST(NULL AS nvarchar(128)) AS LinkId,
       CAST(NULL AS nvarchar(200)) AS AccessKey,
       CAST(NULL AS nvarchar(128)) AS OwnerUserId,
       {SearchTextNormalizer.Sql("CONCAT_WS(N' ', b.Title COLLATE DATABASE_DEFAULT, b.Author COLLATE DATABASE_DEFAULT, b.Code COLLATE DATABASE_DEFAULT, o.Name COLLATE DATABASE_DEFAULT)")} AS SearchText,
       CASE WHEN bo.IsDeleted = 0 AND b.IsDeleted = 0 AND bo.Quantity <> 0 AND LEN(ISNULL(b.Title, N'')) > 0 THEN 1 ELSE 0 END AS IsSearchable
FROM dbo.BookOffices bo
JOIN dbo.Books b ON b.Id = bo.BookId
LEFT JOIN dbo.Offices o ON o.Id = bo.OfficeId";

        public static readonly SearchIndexSource BookOffices = new("BookOffices", SearchDocumentKind.Book, BooksSql("bo.Id"));

        public static readonly SearchIndexSource BooksByBook = new("BooksByBook", SearchDocumentKind.Book, BooksSql("bo.BookId"), filterKeyIsEntityId: false);

        // Drafts and suggestions are only for admins and their author: published polls only.
        public static readonly SearchIndexSource Polls = new("Polls", SearchDocumentKind.Poll, $@"
SELECT p.Id AS FilterKey,
       p.OrganizationId,
       CAST(p.Id AS nvarchar(128)) AS EntityId,
       LEFT(p.Title, 400) AS Title,
       LEFT(p.Description, 400) AS Description,
       CAST(p.Created AS datetime2) AS Date,
       CAST(NULL AS nvarchar(128)) AS PictureId,
       CAST(NULL AS nvarchar(128)) AS LinkId,
       CAST(NULL AS nvarchar(200)) AS AccessKey,
       CAST(NULL AS nvarchar(128)) AS OwnerUserId,
       {SearchTextNormalizer.Sql("CONCAT_WS(N' ', p.Title COLLATE DATABASE_DEFAULT, LEFT(p.Description, 2000) COLLATE DATABASE_DEFAULT)")} AS SearchText,
       CASE WHEN p.IsDeleted = 0 AND p.State = 3 AND LEN(ISNULL(p.Title, N'')) > 0 THEN 1 ELSE 0 END AS IsSearchable
FROM dbo.Polls p");

        public static readonly SearchIndexSource Videos = new("Videos", SearchDocumentKind.Video, $@"
SELECT v.Id AS FilterKey,
       v.OrganizationId,
       CAST(v.Id AS nvarchar(128)) AS EntityId,
       LEFT(v.Title, 400) AS Title,
       LEFT(v.Description, 400) AS Description,
       CAST(v.RecordedOn AS datetime2) AS Date,
       CAST(v.PictureId AS nvarchar(128)) AS PictureId,
       CAST(NULL AS nvarchar(128)) AS LinkId,
       CAST(NULL AS nvarchar(200)) AS AccessKey,
       CAST(NULL AS nvarchar(128)) AS OwnerUserId,
       {SearchTextNormalizer.Sql("CONCAT_WS(N' ', v.Title COLLATE DATABASE_DEFAULT, LEFT(v.Description, 2000) COLLATE DATABASE_DEFAULT)")} AS SearchText,
       CASE WHEN v.IsDeleted = 0 AND LEN(ISNULL(v.Title, N'')) > 0 THEN 1 ELSE 0 END AS IsSearchable
FROM dbo.VideoLibraryItems v");

        // Owner, the category's assignees (access key) and service request admins only.
        public static readonly SearchIndexSource ServiceRequests = new("ServiceRequests", SearchDocumentKind.ServiceRequest, $@"
SELECT r.Id AS FilterKey,
       r.OrganizationId,
       CAST(r.Id AS nvarchar(128)) AS EntityId,
       LEFT(r.Title, 400) AS Title,
       LEFT(CONCAT_WS(N' · ', r.CategoryName COLLATE DATABASE_DEFAULT, st.Title COLLATE DATABASE_DEFAULT), 400) AS Description,
       CAST(r.Created AS datetime2) AS Date,
       CAST(NULL AS nvarchar(128)) AS PictureId,
       CAST(NULL AS nvarchar(128)) AS LinkId,
       CAST(CONCAT(N'category:', r.CategoryName COLLATE DATABASE_DEFAULT) AS nvarchar(200)) AS AccessKey,
       CAST(r.EmployeeId AS nvarchar(128)) AS OwnerUserId,
       {SearchTextNormalizer.Sql("CONCAT_WS(N' ', r.Title COLLATE DATABASE_DEFAULT, r.CategoryName COLLATE DATABASE_DEFAULT, LEFT(r.Description, 2000) COLLATE DATABASE_DEFAULT)")} AS SearchText,
       CASE WHEN r.IsDeleted = 0 AND LEN(ISNULL(r.Title, N'')) > 0 THEN 1 ELSE 0 END AS IsSearchable
FROM dbo.ServiceRequests r
LEFT JOIN dbo.ServiceRequestStatus st ON st.Id = r.StatusId");

        /// <summary>One source per kind: what a full rebuild runs.</summary>
        public static readonly IReadOnlyList<SearchIndexSource> All = new[]
        {
            Posts, People, Events, Kudos, Projects, Groups, BookOffices, Polls, Videos, ServiceRequests
        };
    }
}
