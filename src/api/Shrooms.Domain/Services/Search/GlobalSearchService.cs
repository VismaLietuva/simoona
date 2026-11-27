using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Search;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Polls;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.Wall;
using ConstantsRoles = Shrooms.Contracts.Constants.Roles;
using GroupEntity = Shrooms.DataLayer.EntityModels.Models.Group.Group;
using MultiwallWall = Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall;

namespace Shrooms.Domain.Services.Search
{
    public partial class GlobalSearchService : IGlobalSearchService
    {
        public const int MinSearchLength = 2;
        public const int MaxSearchLength = 50;
        public const int MaxLimit = 50;

        /// <summary>
        /// The names a caller uses to ask for specific sections; they match the response's property names.
        /// </summary>
        public static class Sections
        {
            public const string People = "people";
            public const string Walls = "walls";
            public const string Posts = "posts";
            public const string Events = "events";
            public const string Kudos = "kudos";
            public const string Projects = "projects";
            public const string Groups = "groups";
            public const string Books = "books";
            public const string Polls = "polls";
            public const string Videos = "videos";
            public const string ServiceRequests = "serviceRequests";
        }

        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;
        private readonly ISystemClock _systemClock;
        private readonly IWallService _wallService;
        private readonly DbSet<ApplicationUser> _usersDbSet;
        private readonly DbSet<MultiwallWall> _wallsDbSet;
        private readonly DbSet<Event> _eventsDbSet;
        private readonly DbSet<KudosLog> _kudosLogsDbSet;
        private readonly DbSet<Project> _projectsDbSet;
        private readonly DbSet<GroupEntity> _groupsDbSet;
        private readonly DbSet<BookOffice> _bookOfficesDbSet;
        private readonly DbSet<Poll> _pollsDbSet;
        private readonly DbSet<VideoLibraryItem> _videosDbSet;
        private readonly DbSet<ServiceRequest> _serviceRequestsDbSet;
        private readonly DbSet<ServiceRequestCategory> _serviceRequestCategoriesDbSet;

        public GlobalSearchService(IUnitOfWork2 uow, IPermissionService permissionService, IRoleService roleService, ISystemClock systemClock, IWallService wallService)
        {
            _wallService = wallService;
            _permissionService = permissionService;
            _roleService = roleService;
            _systemClock = systemClock;
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _wallsDbSet = uow.GetDbSet<MultiwallWall>();
            _eventsDbSet = uow.GetDbSet<Event>();
            _kudosLogsDbSet = uow.GetDbSet<KudosLog>();
            _projectsDbSet = uow.GetDbSet<Project>();
            _groupsDbSet = uow.GetDbSet<GroupEntity>();
            _bookOfficesDbSet = uow.GetDbSet<BookOffice>();
            _pollsDbSet = uow.GetDbSet<Poll>();
            _videosDbSet = uow.GetDbSet<VideoLibraryItem>();
            _serviceRequestsDbSet = uow.GetDbSet<ServiceRequest>();
            _serviceRequestCategoriesDbSet = uow.GetDbSet<ServiceRequestCategory>();
        }

        public async Task<GlobalSearchResultsDto> SearchAsync(string search, int limit, UserAndOrganizationDto userOrg, IReadOnlyCollection<string> sections = null, CancellationToken cancellationToken = default)
        {
            var term = (search ?? string.Empty).Trim();
            if (term.Length > MaxSearchLength)
            {
                term = term.Substring(0, MaxSearchLength).Trim();
            }

            if (term.Length < MinSearchLength)
            {
                return new GlobalSearchResultsDto();
            }

            var take = Math.Clamp(limit, 1, MaxLimit);
            var permissions = (await _permissionService.GetUserPermissionsAsync(userOrg.UserId, userOrg.OrganizationId, cancellationToken)).ToHashSet();
            var results = new GlobalSearchResultsDto();

            // A caller that shows one section should not pay for the other ten.
            bool Wants(string section) =>
                sections == null || sections.Count == 0 || sections.Contains(section, StringComparer.OrdinalIgnoreCase);

            // One DbContext, so the lookups run one after another rather than in parallel.
            if (Wants(Sections.People) && permissions.Contains(BasicPermissions.EmployeeList))
            {
                results.People = await SearchPeopleAsync(term, take, userOrg, cancellationToken);
            }

            if (Wants(Sections.Walls) && permissions.Contains(BasicPermissions.Post))
            {
                results.Walls = await SearchWallsAsync(term, take, userOrg, cancellationToken);
            }

            if (Wants(Sections.Posts) && permissions.Contains(BasicPermissions.Post))
            {
                results.Posts = await SearchPostsAsync(term, take, userOrg, cancellationToken);
            }

            if (Wants(Sections.Events) && permissions.Contains(BasicPermissions.Event))
            {
                results.Events = await SearchEventsAsync(term, take, userOrg, cancellationToken);
            }

            if (Wants(Sections.Kudos) && permissions.Contains(BasicPermissions.Kudos))
            {
                results.Kudos = await SearchKudosAsync(term, take, userOrg, cancellationToken);
            }

            if (Wants(Sections.Projects) && permissions.Contains(BasicPermissions.Project))
            {
                results.Projects = await SearchProjectsAsync(term, take, userOrg, cancellationToken);
            }

            if (Wants(Sections.Groups) && permissions.Contains(BasicPermissions.Groups))
            {
                results.Groups = await SearchGroupsAsync(term, take, userOrg, cancellationToken);
            }

            if (Wants(Sections.Books) && permissions.Contains(BasicPermissions.Book))
            {
                results.Books = await SearchBooksAsync(term, take, userOrg, cancellationToken);
            }

            if (Wants(Sections.Polls) && (permissions.Contains(BasicPermissions.Poll) || permissions.Contains(AdministrationPermissions.Poll)))
            {
                results.Polls = await SearchPollsAsync(term, take, userOrg, cancellationToken);
            }

            if (Wants(Sections.Videos) && (permissions.Contains(BasicPermissions.VideoLibrary) || permissions.Contains(AdministrationPermissions.VideoLibrary)))
            {
                results.Videos = await SearchVideosAsync(term, take, userOrg, cancellationToken);
            }

            if (Wants(Sections.ServiceRequests) && permissions.Contains(BasicPermissions.ServiceRequest))
            {
                var seesAll = permissions.Contains(AdministrationPermissions.ServiceRequest);
                results.ServiceRequests = await SearchServiceRequestsAsync(term, take, seesAll, userOrg, cancellationToken);
            }

            return results;
        }

        // Same rule as the employee list: every word must appear in the name or the job title.
        private async Task<GlobalSearchPersonDto[]> SearchPeopleAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            // A search of separators only (",,") has no words, and with no words there is
            // no filter: it would list the first employees as if they had matched.
            var words = term.Split(WebApiConstants.SearchSplitter, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return Array.Empty<GlobalSearchPersonDto>();
            }

            var newUserRoleId = await _roleService.GetRoleIdByNameAsync(ConstantsRoles.NewUser, cancellationToken);

            var query = _usersDbSet
                .Where(user => user.OrganizationId == userOrg.OrganizationId)
                .Where(_roleService.ExcludeUsersWithRole(newUserRoleId));

            foreach (var word in words)
            {
                query = query.Where(user =>
                    user.FirstName.Contains(word) ||
                    user.LastName.Contains(word) ||
                    user.JobPosition.Title.Contains(word));
            }

            return await query
                .OrderByDescending(user => (user.FirstName + " " + user.LastName).StartsWith(term))
                .ThenBy(user => user.FirstName)
                .ThenBy(user => user.LastName)
                .Take(take)
                .Select(user => new GlobalSearchPersonDto
                {
                    Id = user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    JobTitle = user.JobPosition.Title,
                    PictureId = user.PictureId
                })
                .ToArrayAsync(cancellationToken);
        }

        // The walls the wall list offers: the main wall and user-created ones, never event or project walls.
        private async Task<GlobalSearchWallDto[]> SearchWallsAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            return await _wallsDbSet
                .Where(wall => wall.OrganizationId == userOrg.OrganizationId &&
                               (wall.Type == WallType.Main || wall.Type == WallType.UserCreated) &&
                               wall.Name.Contains(term))
                .OrderByDescending(wall => wall.Name.StartsWith(term))
                .ThenBy(wall => wall.Name)
                .Take(take)
                .Select(wall => new GlobalSearchWallDto
                {
                    Id = wall.Id,
                    Name = wall.Name,
                    TotalMembers = wall.Members.Count
                })
                .ToArrayAsync(cancellationToken);
        }

        // The wall search decides which posts the user may see and what counts as a match,
        // so the quick search and the full post search can never disagree.
        private async Task<GlobalSearchPostDto[]> SearchPostsAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var posts = await _wallService.SearchWallAsync(term, userOrg, 1, take, cancellationToken);

            return posts
                .Select(post => new GlobalSearchPostDto
                {
                    Id = post.Id,
                    Text = MatchedText(post, term),
                    AuthorFullName = post.Author?.FullName,
                    WallName = post.WallName,
                    Created = post.Created
                })
                .ToArray();
        }

        // A post is also found through its comments, and a hidden post only through them.
        // The wall search never matches a comment whose author is gone, so such a comment
        // is not offered as the reason the post was found either.
        private static string MatchedText(PostDto post, string term)
        {
            if (!post.IsHidden && Contains(post.MessageBody, term))
            {
                return post.MessageBody;
            }

            var comment = post.Comments?.FirstOrDefault(c => !c.IsHidden && c.Author?.UserId != null && Contains(c.MessageBody, term));
            if (comment != null)
            {
                return comment.MessageBody;
            }

            return post.IsHidden ? string.Empty : post.MessageBody;
        }

        private static bool Contains(string text, string term)
        {
            return text != null && text.Contains(term, StringComparison.OrdinalIgnoreCase);
        }

        // Events still to come are listed ahead of finished ones: the next one first,
        // so it is not the one cut off by the limit, then finished ones, latest first.
        private async Task<GlobalSearchEventDto[]> SearchEventsAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var now = _systemClock.UtcNow;

            return await _eventsDbSet
                .Where(e => e.OrganizationId == userOrg.OrganizationId &&
                            (e.Name.Contains(term) || e.Place.Contains(term)))
                .OrderByDescending(e => e.EndDate >= now)
                .ThenBy(e => e.EndDate >= now ? e.StartDate : (DateTime?)null)
                .ThenByDescending(e => e.StartDate)
                .Take(take)
                .Select(e => new GlobalSearchEventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Place = e.Place
                })
                .ToArrayAsync(cancellationToken);
        }
    }
}
