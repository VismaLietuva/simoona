using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using Shrooms.Premium.Domain.Services.Groups;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Groups
{
    public class GroupKudosWebHookService : IGroupKudosWebHookService
    {
        private readonly DbSet<Organization> _organizationsDbSet;
        private readonly IGroupKudosService _groupKudosService;
        private readonly ILogger<GroupKudosWebHookService> _logger;

        public GroupKudosWebHookService(IUnitOfWork2 uow, IGroupKudosService groupKudosService, ILogger<GroupKudosWebHookService> logger)
        {
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _groupKudosService = groupKudosService;
            _logger = logger;
        }

        public async Task<IList<GroupMonthlyKudosResultDto>> AwardMonthlyGroupKudosAsync(string organizationName, int? year = null, int? month = null)
        {
            if (string.IsNullOrEmpty(organizationName))
            {
                throw new ArgumentNullException(nameof(organizationName));
            }

            if (year.HasValue != month.HasValue)
            {
                throw new ValidationException(
                    ErrorCodes.GroupInvalidKudosPeriod,
                    "Year and month must be given together, or both left out");
            }

            var organizationId = await _organizationsDbSet
                .Where(o => o.ShortName == organizationName)
                .Select(o => o.Id)
                .SingleAsync();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = organizationId,
                UserId = null
            };

            IList<GroupMonthlyKudosResultDto> result;

            if (year.HasValue && month.HasValue)
            {
                result = new List<GroupMonthlyKudosResultDto>
                {
                    await _groupKudosService.AwardMonthlyKudosAsync(userAndOrg, year.Value, month.Value)
                };
            }
            else
            {
                var previousMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1);

                result = await _groupKudosService.AwardOutstandingMonthsAsync(userAndOrg, previousMonth.Year, previousMonth.Month);
            }

            LogOutcome(organizationName, result);

            return result;
        }

        private void LogOutcome(string organizationName, IList<GroupMonthlyKudosResultDto> result)
        {
            if (result.Count == 0)
            {
                _logger.LogInformation("Monthly group kudos for {Organization}: nothing outstanding.", organizationName);
                return;
            }

            foreach (var period in result)
            {
                if (period.AlreadyAwarded)
                {
                    _logger.LogInformation(
                        "Monthly group kudos for {Organization} {Year}-{Month:00} were already awarded; nothing written.",
                        organizationName,
                        period.Year,
                        period.Month);
                }
                else
                {
                    _logger.LogInformation(
                        "Monthly group kudos for {Organization} {Year}-{Month:00}: {AwardedCount} pending logs, {TotalAmount} points.",
                        organizationName,
                        period.Year,
                        period.Month,
                        period.AwardedCount,
                        period.TotalAmount);
                }
            }
        }
    }
}
