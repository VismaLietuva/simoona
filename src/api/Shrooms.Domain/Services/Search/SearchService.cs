using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Search;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.DAL.Search;
using Shrooms.Domain.Services.FeatureFlags;
using Shrooms.Domain.Services.Permissions;

namespace Shrooms.Domain.Services.Search
{
    public interface ISearchService
    {
        Task<bool> IsEnabledAsync(UserAndOrganizationDto userOrg);

        /// <summary>
        /// <paramref name="context"/> is a section key ("kudos", "people", ...) or null
        /// for every section; null means an unknown context.
        /// </summary>
        Task<SearchResultDto> SearchAsync(UserAndOrganizationDto userOrg, string query, string context, CancellationToken cancellationToken = default);

        Task RebuildIndexAsync(CancellationToken cancellationToken = default);
    }

    public class SearchService : ISearchService
    {
        public const int MinQueryLength = 2;
        public const int MaxQueryLength = 100;
        public const int PreviewPerKind = 3;
        public const int ResultsPerKind = 20;

        private const int MaxTerms = 6;
        private const int MaxTermLength = 40;

        private static readonly Regex Words = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled);

        private static readonly IReadOnlyDictionary<SearchDocumentKind, (string Key, string[] Permissions)> Sections =
            new Dictionary<SearchDocumentKind, (string, string[])>
            {
                [SearchDocumentKind.Post] = ("posts", new[] { BasicPermissions.Post }),
                [SearchDocumentKind.Person] = ("people", new[] { BasicPermissions.EmployeeList }),
                [SearchDocumentKind.Event] = ("events", new[] { BasicPermissions.Event }),
                [SearchDocumentKind.Kudos] = ("kudos", new[] { BasicPermissions.Kudos }),
                [SearchDocumentKind.Project] = ("projects", new[] { BasicPermissions.Project }),
                [SearchDocumentKind.Group] = ("groups", new[] { BasicPermissions.Groups }),
                [SearchDocumentKind.Book] = ("books", new[] { BasicPermissions.Book }),
                [SearchDocumentKind.Poll] = ("polls", new[] { BasicPermissions.Poll, AdministrationPermissions.Poll }),
                [SearchDocumentKind.Video] = ("videos", new[] { BasicPermissions.VideoLibrary, AdministrationPermissions.VideoLibrary }),
                [SearchDocumentKind.ServiceRequest] = ("serviceRequests", new[] { BasicPermissions.ServiceRequest })
            };

        private readonly IDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IFeatureFlagService _featureFlagService;
        private readonly ILogger<SearchService> _logger;

        public SearchService(IDbContext context, IPermissionService permissionService, IFeatureFlagService featureFlagService, ILogger<SearchService> logger)
        {
            _context = context;
            _permissionService = permissionService;
            _featureFlagService = featureFlagService;
            _logger = logger;
        }

        public async Task<bool> IsEnabledAsync(UserAndOrganizationDto userOrg)
        {
            var enabled = await _featureFlagService.GetEnabledAsync(userOrg);
            return enabled.Contains(SearchIndexing.FeatureFlagName, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<SearchResultDto> SearchAsync(UserAndOrganizationDto userOrg, string query, string context, CancellationToken cancellationToken = default)
        {
            var empty = new SearchResultDto { Groups = Array.Empty<SearchResultGroupDto>() };
            var terms = ToTerms(query);
            if (terms.Count == 0)
            {
                return empty;
            }

            SearchDocumentKind? onlyKind = null;
            if (!string.IsNullOrEmpty(context) && context != "all")
            {
                var section = Sections.FirstOrDefault(pair => string.Equals(pair.Value.Key, context, StringComparison.OrdinalIgnoreCase));
                if (section.Value.Key == null)
                {
                    return empty;
                }

                onlyKind = section.Key;
            }

            var permissions = (await _permissionService.GetUserPermissionsAsync(userOrg.UserId, userOrg.OrganizationId, cancellationToken)).ToHashSet();
            var kinds = Sections
                .Where(pair => pair.Value.Permissions.Any(permissions.Contains))
                .Select(pair => pair.Key)
                .Where(kind => onlyKind == null || kind == onlyKind)
                .ToList();
            if (kinds.Count == 0)
            {
                return empty;
            }

            var unrestricted = permissions.Contains(AdministrationPermissions.ServiceRequest)
                ? new[] { SearchDocumentKind.ServiceRequest }
                : Array.Empty<SearchDocumentKind>();

            var hits = await SearchIndexReader.SearchAsync(
                (ShroomsDbContext)_context,
                new SearchIndexQuery
                {
                    OrganizationId = userOrg.OrganizationId,
                    UserId = userOrg.UserId,
                    Terms = terms,
                    Kinds = kinds,
                    UnrestrictedKinds = unrestricted,
                    CanSeeEventWalls = permissions.Contains(BasicPermissions.Event),
                    LimitPerKind = onlyKind == null ? PreviewPerKind : ResultsPerKind
                },
                cancellationToken);

            return new SearchResultDto
            {
                Groups = hits
                    .GroupBy(hit => (SearchDocumentKind)hit.Kind)
                    .Select(group => new SearchResultGroupDto
                    {
                        Kind = Sections[group.Key].Key,
                        Total = group.First().Total,
                        Items = group.Select(hit => new SearchResultItemDto
                        {
                            Id = hit.EntityId,
                            Title = hit.Title,
                            Description = hit.Description,
                            Date = hit.Date,
                            PictureId = hit.PictureId,
                            LinkId = hit.LinkId
                        }).ToList()
                    })
                    .ToList()
            };
        }

        public async Task RebuildIndexAsync(CancellationToken cancellationToken = default)
        {
            var context = (ShroomsDbContext)_context;
            var previousTimeout = context.Database.GetCommandTimeout();
            context.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            try
            {
                foreach (var source in SearchIndexSources.All)
                {
                    var changed = await SearchIndexWriter.RebuildAsync(context, source, cancellationToken);
                    _logger.LogInformation("Search rebuild of {SearchSource} changed {Rows} rows.", source.Name, changed);
                }
            }
            finally
            {
                context.Database.SetCommandTimeout(previousTimeout);
            }
        }

        internal static IReadOnlyList<string> ToTerms(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<string>();
            }

            var trimmed = query.Trim();
            if (trimmed.Length > MaxQueryLength)
            {
                trimmed = trimmed[..MaxQueryLength];
            }

            var terms = Words.Matches(SearchTextNormalizer.Normalize(trimmed))
                .Select(match => match.Value.Length > MaxTermLength ? match.Value[..MaxTermLength] : match.Value)
                .Distinct()
                .Take(MaxTerms)
                .ToList();

            return terms.Sum(term => term.Length) < MinQueryLength ? Array.Empty<string>() : terms;
        }
    }
}
