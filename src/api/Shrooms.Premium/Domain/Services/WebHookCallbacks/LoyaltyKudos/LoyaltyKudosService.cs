using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.Domain.Services.Email.Kudos;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public class LoyaltyKudosService : ILoyaltyKudosService
    {
        private static readonly SemaphoreSlim _concurrencyLock = new SemaphoreSlim(1, 1);

        private const string LoyaltyKudosTypeName = "Loyalty";

        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<KudosLog> _kudosLogsDbSet;
        private readonly IDbSet<KudosType> _kudosTypesDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly ILogger _logger;
        private readonly IAsyncRunner _asyncRunner;
        private readonly IMapper _mapper;
        private readonly ILoyaltyKudosCalculator _loyaltyKudosCalculator;

        public LoyaltyKudosService(IUnitOfWork2 uow, ILogger logger, IAsyncRunner asyncRunner, IMapper mapper, ILoyaltyKudosCalculator loyaltyKudosCalculator)
        {
            _logger = logger;
            _asyncRunner = asyncRunner;
            _loyaltyKudosCalculator = loyaltyKudosCalculator;
            _mapper = mapper;
            _uow = uow;
            _kudosLogsDbSet = uow.GetDbSet<KudosLog>();
            _kudosTypesDbSet = uow.GetDbSet<KudosType>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
        }

        public async Task AwardEmployeesWithKudosAsync(string organizationName)
        {
            var awardedEmployees = new List<AwardedKudosEmployeeDto>();

            await _concurrencyLock.WaitAsync();

            try
            {
                if (string.IsNullOrEmpty(organizationName))
                {
                    throw new ArgumentNullException(nameof(organizationName));
                }

                var organization = await _organizationsDbSet
                    .Where(o => o.ShortName == organizationName)
                    .Select(o => new
                    {
                        o.Id,
                        o.KudosYearlyMultipliers
                    })
                    .SingleAsync();

                var kudosYearlyMultipliers = GetKudosYearlyMultipliersArray(organization.KudosYearlyMultipliers);
                if (kudosYearlyMultipliers == null)
                {
                    return;
                }

                var loyaltyType = await _kudosTypesDbSet.SingleOrDefaultAsync(t => t.Name == LoyaltyKudosTypeName);

                var loyaltyKudosLog = await (from u in _usersDbSet
                                       where u.OrganizationId == organization.Id && u.EmploymentDate.HasValue
                                       join kl in _kudosLogsDbSet
                                           on new { employeeId = u.Id, orgId = organization.Id, Status = KudosStatus.Approved, KudosTypeName = LoyaltyKudosTypeName }
                                           equals new { employeeId = kl.EmployeeId, orgId = kl.OrganizationId, kl.Status, kl.KudosTypeName }
                                           into klx
                                       from kl in klx.DefaultIfEmpty()
                                       select new
                                       {
                                           Employee = u,
                                           KudosAddedDate = kl == null ? (DateTime?)null : kl.Created
                                       }).ToListAsync();

                var employeesReceivedLoyaltyKudos = await loyaltyKudosLog
                    .GroupBy(l => l.Employee)
                    .Select(l => new EmployeeLoyaltyKudosDto
                    {
                        Employee = l.Key,
                        AwardedEmploymentYears = l
                            .Where(log => log.Employee.EmploymentDate != null && log.KudosAddedDate != null)
                            .Select(log => GetLoyaltyKudosEmploymentYear(log.Employee.EmploymentDate, log.KudosAddedDate.Value)),

                        AwardedLoyaltyKudosCount = l.Count(log => log.KudosAddedDate != null)
                    })
                    .ToListAsync();

                foreach (var employeeLoyaltyKudos in employeesReceivedLoyaltyKudos)
                {
                    try
                    {
                        var loyaltyKudosLogList = _loyaltyKudosCalculator.GetEmployeeLoyaltyKudosLog(employeeLoyaltyKudos, loyaltyType, organization.Id, kudosYearlyMultipliers);
                        var awardedKudosEmployeeList = _mapper.Map<List<AwardedKudosEmployeeDto>>(loyaltyKudosLogList);
                        awardedEmployees.AddRange(awardedKudosEmployeeList);

                        loyaltyKudosLogList.ForEach(l => _kudosLogsDbSet.Add(l));
                    }
                    catch (ArgumentException e)
                    {
                        _logger.Error(e);
                    }
                }

                await _uow.SaveChangesAsync(false);
                _asyncRunner.Run<IKudosPremiumNotificationService>(async notifier => await notifier.SendLoyaltyBotNotificationAsync(awardedEmployees), _uow.ConnectionName);
            }
            finally
            {
                _concurrencyLock.Release();
            }
        }

        private static int[] GetKudosYearlyMultipliersArray(string multipliers)
        {
            if (string.IsNullOrEmpty(multipliers))
            {
                return null;
            }

            try
            {
                return Array.ConvertAll(multipliers.Split(';'), int.Parse);
            }
            catch
            {
                return null;
            }
        }

        private static int GetLoyaltyKudosEmploymentYear(DateTime? employmentDate, DateTime loyaltyAddedDate)
        {
            employmentDate ??= DateTime.UtcNow;

            var employmentYear = loyaltyAddedDate.Year - employmentDate.Value.Year;
            if (loyaltyAddedDate < employmentDate.Value.AddYears(employmentYear))
            {
                employmentYear = employmentYear >= 1 ? employmentYear - 1 : employmentYear;
            }

            return employmentYear;
        }
    }
}