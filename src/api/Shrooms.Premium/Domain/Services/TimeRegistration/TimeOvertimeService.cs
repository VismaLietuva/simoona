using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;
using Shrooms.Premium.Domain.Services.Email.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public class TimeOvertimeService : ITimeOvertimeService
    {
        public const int MaxMinutesPerDay = 12 * 60;
        public const int MaxDays = 31;
        private const int TeamHistoryDays = 62;
        private const int ListLimit = 200;

        private readonly IUnitOfWork2 _uow;
        private readonly ITimeRegistrationSupport _support;
        private readonly ISystemClock _clock;
        private readonly ILogger<TimeOvertimeService> _logger;
        private readonly ITimeOvertimeNotificationService _notificationService;
        private readonly DbSet<TimeOvertimeRequest> _requests;

        public TimeOvertimeService(
            IUnitOfWork2 uow,
            ITimeRegistrationSupport support,
            ISystemClock clock,
            ILogger<TimeOvertimeService> logger,
            ITimeOvertimeNotificationService notificationService)
        {
            _uow = uow;
            _support = support;
            _clock = clock;
            _logger = logger;
            _notificationService = notificationService;
            _requests = uow.GetDbSet<TimeOvertimeRequest>();
        }

        public async Task<TimeOvertimeRequestDto> GetForReviewAsync(int id, UserAndOrganizationDto userOrg)
        {
            var request = await FindAsync(id, userOrg);
            await _support.ResolveAsync(userOrg, request.UserId);
            return (await ListAsync(_requests.Where(row => row.Id == request.Id))).Single();
        }

        public async Task<IEnumerable<TimeOvertimeRequestDto>> GetForEmployeeAsync(string employeeId, UserAndOrganizationDto userOrg)
        {
            var actor = await _support.ResolveAsync(userOrg, employeeId);
            return await ListAsync(_requests
                .Where(request => request.OrganizationId == actor.OrganizationId && request.UserId == actor.UserId));
        }

        public async Task<IEnumerable<TimeOvertimeRequestDto>> GetTeamAsync(UserAndOrganizationDto userOrg)
        {
            var since = (await _support.TodayAsync(userOrg.OrganizationId)).AddDays(-TeamHistoryDays);
            return await ListAsync(_requests
                .Where(request => request.OrganizationId == userOrg.OrganizationId && request.User.ManagerId == userOrg.UserId)
                .Where(request => request.Status == TimeOvertimeRequestStatus.Pending || request.DateTo >= since));
        }

        public async Task<int> CreateAsync(TimeOvertimeRequestInputDto input, UserAndOrganizationDto userOrg)
        {
            var actor = await _support.ResolveAsync(userOrg, input.EmployeeId);
            var from = TimeMonth.ParseDay(input.DateFrom);
            var to = TimeMonth.ParseDay(input.DateTo);
            if (to < from || (to - from).TotalDays >= MaxDays)
            {
                throw Invalid($"Pick from 1 to {MaxDays} days");
            }

            if (input.MinutesPerDay < 1 || input.MinutesPerDay > MaxMinutesPerDay)
            {
                throw Invalid($"Overtime must be up to {MaxMinutesPerDay / 60} hours a day");
            }

            var request = new TimeOvertimeRequest
            {
                OrganizationId = actor.OrganizationId,
                UserId = actor.UserId,
                DateFrom = from,
                DateTo = to,
                MinutesPerDay = input.MinutesPerDay,
                Reason = Text(input.Reason, required: true),
                Status = TimeOvertimeRequestStatus.Pending,
                OrderedByManager = !actor.IsSelf,
                RequestedById = userOrg.UserId
            };
            _requests.Add(request);
            await _uow.SaveChangesAsync(userOrg.UserId);
            await NotifyAsync(request, userOrg);
            return request.Id;
        }

        public Task ApproveAsync(int id, UserAndOrganizationDto userOrg)
        {
            return DecideAsync(id, TimeOvertimeRequestStatus.Approved, null, userOrg);
        }

        public Task RejectAsync(int id, string comment, UserAndOrganizationDto userOrg)
        {
            return DecideAsync(id, TimeOvertimeRequestStatus.Rejected, Text(comment, required: true), userOrg);
        }

        public async Task CancelAsync(int id, UserAndOrganizationDto userOrg)
        {
            var request = await FindAsync(id, userOrg);
            if (request.Status == TimeOvertimeRequestStatus.Pending)
            {
                if (!TimeRegistrationRules.SameUser(request.RequestedById, userOrg.UserId))
                {
                    throw new UnauthorizedException();
                }
            }
            else if (request.Status == TimeOvertimeRequestStatus.Approved)
            {
                await ThrowUnlessManagerAsync(request, userOrg);
            }
            else
            {
                throw WrongStatus("This request is already closed");
            }

            await CloseAsync(request, TimeOvertimeRequestStatus.Cancelled, null, userOrg);
        }

        private async Task DecideAsync(int id, TimeOvertimeRequestStatus status, string comment, UserAndOrganizationDto userOrg)
        {
            var request = await FindAsync(id, userOrg);
            if (request.Status != TimeOvertimeRequestStatus.Pending)
            {
                throw WrongStatus("This request has already been decided");
            }

            if (request.OrderedByManager)
            {
                if (!TimeRegistrationRules.SameUser(request.UserId, userOrg.UserId))
                {
                    throw new UnauthorizedException();
                }
            }
            else
            {
                await ThrowUnlessManagerAsync(request, userOrg);
            }

            await CloseAsync(request, status, comment, userOrg);
        }

        private async Task CloseAsync(TimeOvertimeRequest request, TimeOvertimeRequestStatus status, string comment, UserAndOrganizationDto userOrg)
        {
            request.Status = status;
            request.DecidedById = userOrg.UserId;
            request.DecidedAt = _clock.UtcNow;
            request.ReviewComment = comment;
            await _uow.SaveChangesAsync(userOrg.UserId);
            await NotifyAsync(request, userOrg);
        }

        private async Task ThrowUnlessManagerAsync(TimeOvertimeRequest request, UserAndOrganizationDto userOrg)
        {
            if ((await _support.ResolveAsync(userOrg, request.UserId)).IsSelf)
            {
                throw new UnauthorizedException();
            }
        }

        private async Task<TimeOvertimeRequest> FindAsync(int id, UserAndOrganizationDto userOrg)
        {
            return await _requests.FirstOrDefaultAsync(request => request.OrganizationId == userOrg.OrganizationId && request.Id == id)
                   ?? throw new ValidationException(ErrorCodes.TimeRegistrationNotFound, "Overtime request not found");
        }

        private async Task<IEnumerable<TimeOvertimeRequestDto>> ListAsync(IQueryable<TimeOvertimeRequest> query)
        {
            var rows = await query
                .AsNoTracking()
                .OrderBy(request => request.Status == TimeOvertimeRequestStatus.Pending ? 0 : 1)
                .ThenByDescending(request => request.DateFrom)
                .ThenByDescending(request => request.Id)
                .Take(ListLimit)
                .Select(request => new
                {
                    request.Id,
                    request.UserId,
                    request.User.FirstName,
                    request.User.LastName,
                    request.User.PictureId,
                    request.DateFrom,
                    request.DateTo,
                    request.MinutesPerDay,
                    request.Reason,
                    request.Status,
                    request.OrderedByManager,
                    request.RequestedById,
                    RequestedByFirstName = request.RequestedBy.FirstName,
                    RequestedByLastName = request.RequestedBy.LastName,
                    request.Created,
                    DecidedByFirstName = request.DecidedBy.FirstName,
                    DecidedByLastName = request.DecidedBy.LastName,
                    request.DecidedAt,
                    request.ReviewComment
                })
                .ToListAsync();

            return rows.Select(row => new TimeOvertimeRequestDto
            {
                Id = row.Id,
                EmployeeId = row.UserId,
                EmployeeName = TimeTableService.FullName(row.FirstName, row.LastName),
                EmployeePictureId = row.PictureId,
                DateFrom = TimeMonth.ToDay(row.DateFrom),
                DateTo = TimeMonth.ToDay(row.DateTo),
                MinutesPerDay = row.MinutesPerDay,
                Reason = row.Reason,
                Status = row.Status.ToString().ToLowerInvariant(),
                OrderedByManager = row.OrderedByManager,
                CreatedById = row.RequestedById,
                CreatedByName = TimeTableService.FullName(row.RequestedByFirstName, row.RequestedByLastName),
                CreatedAt = DateTime.SpecifyKind(row.Created, DateTimeKind.Utc),
                DecidedByName = row.DecidedAt == null ? null : TimeTableService.FullName(row.DecidedByFirstName, row.DecidedByLastName),
                DecidedAt = row.DecidedAt == null ? null : DateTime.SpecifyKind(row.DecidedAt.Value, DateTimeKind.Utc),
                ReviewComment = row.ReviewComment
            }).ToList();
        }

        private async Task NotifyAsync(TimeOvertimeRequest request, UserAndOrganizationDto actor)
        {
            try
            {
                await _notificationService.NotifyAsync(request, actor);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Overtime notification failed");
            }
        }

        private static string Text(string value, bool required)
        {
            var text = value?.Trim() ?? string.Empty;
            if (required && text.Length == 0)
            {
                throw Invalid("Enter a reason");
            }

            if (text.Length > TimeOvertimeRequest.MaxReasonLength)
            {
                throw Invalid($"Use up to {TimeOvertimeRequest.MaxReasonLength} characters");
            }

            return text;
        }

        private static ValidationException Invalid(string message)
        {
            return new ValidationException(ErrorCodes.TimeRegistrationInvalid, message);
        }

        private static ValidationException WrongStatus(string message)
        {
            return new ValidationException(ErrorCodes.TimeRegistrationWrongStatus, message);
        }
    }
}
