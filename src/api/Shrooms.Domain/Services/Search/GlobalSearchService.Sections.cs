using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Search;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Domain.Services.Search
{
    // The sections that share one result shape: a title, one line of detail and a date.
    // Each mirrors what its own page lists, so search never shows more than the page does.
    public partial class GlobalSearchService
    {
        private const int MaxDescriptionLength = 200;

        // Found by its comment. As on the kudos page: approved only, no minus or refund
        // entries, no basket donations. A comment the receiver hid is never matched, so
        // an entry with a hidden comment is not found at all.
        private async Task<GlobalSearchItemDto[]> SearchKudosAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var rows = await _kudosLogsDbSet
                .Where(log => log.OrganizationId == userOrg.OrganizationId &&
                              log.Status == KudosStatus.Approved &&
                              log.KudosSystemType != KudosTypeEnum.Minus &&
                              log.KudosSystemType != KudosTypeEnum.Refund &&
                              log.KudosBasketId == null)
                .Where(log => !log.IsCommentHidden && log.Comments.Contains(term))
                .OrderByDescending(log => log.Created)
                .Take(take)
                .Select(log => new
                {
                    log.Id,
                    Receiver = log.Employee.FirstName + " " + log.Employee.LastName,
                    Sender = _usersDbSet
                        .Where(sender => sender.Id == log.CreatedBy)
                        .Select(sender => sender.FirstName + " " + sender.LastName)
                        .FirstOrDefault(),
                    log.KudosTypeName,
                    Comment = log.IsCommentHidden ? null : log.Comments,
                    log.Created,
                    log.EmployeeId
                })
                .ToListAsync(cancellationToken);

            return rows
                .Select(row => new GlobalSearchItemDto
                {
                    Id = row.Id.ToString(),
                    Title = string.IsNullOrWhiteSpace(row.Sender) ? row.Receiver : $"{row.Sender} → {row.Receiver}",
                    Description = Shorten(string.IsNullOrWhiteSpace(row.Comment) ? row.KudosTypeName : $"{row.KudosTypeName} · {row.Comment}"),
                    Date = row.Created,
                    LinkId = row.EmployeeId
                })
                .ToArray();
        }

        private async Task<GlobalSearchItemDto[]> SearchProjectsAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var rows = await _projectsDbSet
                .Where(project => project.OrganizationId == userOrg.OrganizationId && project.Name.Contains(term))
                .OrderByDescending(project => project.Name.StartsWith(term))
                .ThenBy(project => project.Name)
                .Take(take)
                .Select(project => new { project.Id, project.Name, project.Desc })
                .ToListAsync(cancellationToken);

            return rows
                .Select(row => new GlobalSearchItemDto { Id = row.Id.ToString(), Title = row.Name, Description = Shorten(row.Desc) })
                .ToArray();
        }

        // A group waiting for approval is only for its members and the admins.
        private async Task<GlobalSearchItemDto[]> SearchGroupsAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var rows = await _groupsDbSet
                .Where(group => group.OrganizationId == userOrg.OrganizationId &&
                                group.Status == GroupStatus.Approved &&
                                group.Name.Contains(term))
                .OrderByDescending(group => group.Name.StartsWith(term))
                .ThenBy(group => group.Name)
                .Take(take)
                .Select(group => new { group.Id, group.Name, group.Description })
                .ToListAsync(cancellationToken);

            return rows
                .Select(row => new GlobalSearchItemDto { Id = row.Id.ToString(), Title = row.Name, Description = Shorten(row.Description) })
                .ToArray();
        }

        // The book page is per office copy, so each copy an office still holds is its own result.
        private async Task<GlobalSearchItemDto[]> SearchBooksAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var rows = await _bookOfficesDbSet
                .Where(copy => copy.OrganizationId == userOrg.OrganizationId &&
                               copy.Quantity != 0 &&
                               copy.Book.Title.Contains(term))
                .OrderByDescending(copy => copy.Book.Title.StartsWith(term))
                .ThenBy(copy => copy.Book.Title)
                .Take(take)
                .Select(copy => new { copy.Id, copy.Book.Title, copy.Book.Author, Office = copy.Office.Name })
                .ToListAsync(cancellationToken);

            return rows
                .Select(row => new GlobalSearchItemDto
                {
                    Id = row.Id.ToString(),
                    Title = row.Title,
                    Description = string.Join(" · ", new[] { row.Author, row.Office }.Where(part => !string.IsNullOrWhiteSpace(part)))
                })
                .ToArray();
        }

        // Drafts and suggestions are for their author and the admins: published polls only.
        private async Task<GlobalSearchItemDto[]> SearchPollsAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var rows = await _pollsDbSet
                .Where(poll => poll.OrganizationId == userOrg.OrganizationId &&
                               poll.State == PollState.Published &&
                               poll.Title.Contains(term))
                .OrderByDescending(poll => poll.Created)
                .Take(take)
                .Select(poll => new { poll.Id, poll.Title, poll.Description, poll.Created })
                .ToListAsync(cancellationToken);

            return rows
                .Select(row => new GlobalSearchItemDto { Id = row.Id.ToString(), Title = row.Title, Description = Shorten(row.Description), Date = row.Created })
                .ToArray();
        }

        private async Task<GlobalSearchItemDto[]> SearchVideosAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var rows = await _videosDbSet
                .Where(video => video.OrganizationId == userOrg.OrganizationId && video.Title.Contains(term))
                .OrderByDescending(video => video.RecordedOn)
                .Take(take)
                .Select(video => new { video.Id, video.Title, video.Description, video.RecordedOn })
                .ToListAsync(cancellationToken);

            return rows
                .Select(row => new GlobalSearchItemDto
                {
                    Id = row.Id.ToString(),
                    Title = row.Title,
                    Description = Shorten(row.Description),
                    Date = row.RecordedOn.ToDateTime(TimeOnly.MinValue)
                })
                .ToArray();
        }

        // A request is for the person who raised it, the people its category is assigned to,
        // and the service request admins.
        private async Task<GlobalSearchItemDto[]> SearchServiceRequestsAsync(string term, int take, bool seesAll, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var rows = await _serviceRequestsDbSet
                .Where(request => request.OrganizationId == userOrg.OrganizationId && request.Title.Contains(term))
                .Where(request => seesAll ||
                                  request.EmployeeId == userOrg.UserId ||
                                  _serviceRequestCategoriesDbSet.Any(category =>
                                      category.Name == request.CategoryName &&
                                      category.Assignees.Any(assignee => assignee.Id == userOrg.UserId)))
                .OrderByDescending(request => request.Created)
                .Take(take)
                .Select(request => new { request.Id, request.Title, request.CategoryName, Status = request.Status.Title, request.Created })
                .ToListAsync(cancellationToken);

            return rows
                .Select(row => new GlobalSearchItemDto
                {
                    Id = row.Id.ToString(),
                    Title = row.Title,
                    Description = string.Join(" · ", new[] { row.CategoryName, row.Status }.Where(part => !string.IsNullOrWhiteSpace(part))),
                    Date = row.Created
                })
                .ToArray();
        }

        private static string Shorten(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var flat = string.Join(" ", text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
            return flat.Length <= MaxDescriptionLength ? flat : flat.Substring(0, MaxDescriptionLength).TrimEnd() + "…";
        }
    }
}
