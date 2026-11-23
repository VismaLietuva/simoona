using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Domain.Services.Events
{
    // The shared-event cards of a feed page in one query, with the same rules as
    // Events/Details and Events/Options (see EventService.GetEventDetailsAsync).
    public class SharedEventSummaryService : ISharedEventSummaryService
    {
        public const int MaxAttendees = 3;

        private readonly IPermissionService _permissionService;
        private readonly DbSet<Event> _eventsDbSet;

        public SharedEventSummaryService(IUnitOfWork2 uow, IPermissionService permissionService)
        {
            _permissionService = permissionService;
            _eventsDbSet = uow.GetDbSet<Event>();
        }

        public async Task<IReadOnlyDictionary<Guid, SharedEventSummaryDto>> GetSummariesAsync(
            IReadOnlyCollection<Guid> eventIds,
            UserAndOrganizationDto userOrg,
            CancellationToken cancellationToken = default)
        {
            var empty = new Dictionary<Guid, SharedEventSummaryDto>();

            if (eventIds.Count == 0)
            {
                return empty;
            }

            // Events/Details is gated on the same permission; without it the card falls back to a link.
            if (!await _permissionService.UserHasPermissionAsync(userOrg, BasicPermissions.Event, cancellationToken))
            {
                return empty;
            }

            // Without EventUsers, Details lists only the caller's own row, which the card leaves out anyway.
            var canSeeAll = await _permissionService.UserHasPermissionAsync(userOrg, BasicPermissions.EventUsers, cancellationToken);

            var userId = userOrg.UserId;
            var ids = eventIds.Distinct().ToList();
            const int attending = (int)AttendingStatus.Attending;
            const int attendingVirtually = (int)AttendingStatus.AttendingVirtually;

            var rows = await _eventsDbSet
                .AsNoTracking()
                .Where(e => e.OrganizationId == userOrg.OrganizationId && ids.Contains(e.Id))
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.ImageName,
                    e.Place,
                    e.StartDate,
                    e.EndDate,
                    e.RegistrationDeadline,
                    e.MaxParticipants,
                    e.MaxChoices,
                    InPersonCount = e.EventParticipants.Count(p => p.AttendStatus == attending),
                    VirtualCount = e.EventParticipants.Count(p => p.AttendStatus == attendingVirtually),
                    MyStatus = e.EventParticipants
                        .Where(p => p.ApplicationUserId == userId)
                        .Select(p => (int?)p.AttendStatus)
                        .FirstOrDefault(),
                    HasQuestions = e.EventQuestions.Any(),
                    HasFlatOptions = e.EventOptions.Any(o => o.QuestionId == null),
                    Attendees = e.EventParticipants
                        .Where(p => canSeeAll &&
                                    p.ApplicationUserId != userId &&
                                    (p.AttendStatus == attending || p.AttendStatus == attendingVirtually))
                        .OrderBy(p => p.Id)
                        .Take(MaxAttendees)
                        // A deleted account keeps its participant row but loses the user, as in Events/Details.
                        .Select(p => p.ApplicationUser == null
                            ? new SharedEventAttendeeDto { UserId = string.Empty }
                            : new SharedEventAttendeeDto
                            {
                                UserId = p.ApplicationUserId,
                                FullName = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName,
                                ImageName = p.ApplicationUser.PictureId
                            })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(
                row => row.Id,
                row =>
                {
                    var myStatus = (AttendingStatus?)row.MyStatus ?? AttendingStatus.Idle;

                    return new SharedEventSummaryDto
                    {
                        Id = row.Id,
                        Name = row.Name,
                        ImageName = row.ImageName,
                        Location = row.Place,
                        StartDate = row.StartDate,
                        EndDate = row.EndDate,
                        RegistrationDeadlineDate = row.RegistrationDeadline,
                        IsFull = row.InPersonCount >= row.MaxParticipants,
                        IsAttending = myStatus is AttendingStatus.Attending or AttendingStatus.AttendingVirtually,
                        HasResponse = myStatus != AttendingStatus.Idle,
                        GoingCount = row.InPersonCount + row.VirtualCount,
                        Attendees = row.Attendees,
                        // Flat options count only when the event asks for a pick, the same rule as the wizard.
                        NeedsSignUpSteps = row.HasQuestions || (row.MaxChoices > 0 && row.HasFlatOptions)
                    };
                });
        }
    }
}
