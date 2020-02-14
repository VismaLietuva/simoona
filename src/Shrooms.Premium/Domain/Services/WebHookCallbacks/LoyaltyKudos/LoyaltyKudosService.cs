using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.Domain.Services.Email.Kudos;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public class LoyaltyKudosService : ILoyaltyKudosService
    {
        private const string LoyaltyKudosTypeName = "Loyalty";
        private static readonly object _concurrencyLock = new object();

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

        public void AwardEmployeesWithKudos(string organizationName)
        {
            var awardedEmployees = new List<AwardedKudosEmployeeDTO>();
            lock (_concurrencyLock)
            {
                if (string.IsNullOrEmpty(organizationName))
                {
                    throw new ArgumentNullException(nameof(organizationName));
                }

                var organization = _organizationsDbSet
                    .Where(o => o.ShortName == organizationName)
                    .Select(o => new
                    {
                        o.Id,
                        o.KudosYearlyMultipliers
                    })
                    .Single();

                var kudosYearlyMultipliers = GetKudosYearlyMultipliersArray(organization.KudosYearlyMultipliers);
                if (kudosYearlyMultipliers == null)
                {
                    return;
                }

                var loyaltyType = _kudosTypesDbSet.SingleOrDefault(t => t.Name == LoyaltyKudosTypeName);

                var loyaltyKudosLog = (from u in _usersDbSet
                                       where u.OrganizationId == organization.Id && u.EmploymentDate.HasValue
                                       join kl in _kudosLogsDbSet on u.Id equals kl.EmployeeId into klx
                                       from kl in klx.DefaultIfEmpty()
                                       where kl == null || (kl.OrganizationId == organization.Id && kl.Status == KudosStatus.Approved && kl.KudosTypeName == LoyaltyKudosTypeName)
                                       select new
                                       {
                                           Employee = u,
                                           KudosAddedDate = kl != null ? (DateTime?)(kl.Created) : null
                                       }).ToList();

                var employeesReceivedLoyaltyKudos = loyaltyKudosLog
                    .GroupBy(l => l.Employee)
                    .Select(l => new EmployeeLoyaltyKudosDTO
                    {
                        Employee = l.Key,
                        AwardedEmploymentYears = l.Where(log => log.Employee.EmploymentDate != null && log.KudosAddedDate != null).Select(log => GetLoyaltyKudosEmploymentYear(log.Employee.EmploymentDate.Value, log.KudosAddedDate.Value)),
                        AwardedLoyaltyKudosCount = l.Count(log => log.KudosAddedDate != null)
                    })
                    .ToList();

                foreach (var employeeLoyaltyKudos in employeesReceivedLoyaltyKudos)
                {
                    try
                    {
                        var loyaltyKudosLogList = _loyaltyKudosCalculator.GetEmployeeLoyaltyKudosLog(employeeLoyaltyKudos, loyaltyType, organization.Id, kudosYearlyMultipliers);
                        var awardedKudosEmployeeList = _mapper.Map<List<AwardedKudosEmployeeDTO>>(loyaltyKudosLogList);
                        awardedEmployees.AddRange(awardedKudosEmployeeList);

                        loyaltyKudosLogList.ForEach(l => _kudosLogsDbSet.Add(l));
                    }
                    catch (ArgumentException e)
                    {
                        _logger.Error(e);
                    }
                }

                _uow.SaveChanges(false);
                _asyncRunner.Run<IKudosPremiumNotificationService>(ntf => ntf.SendLoyaltyBotNotification(awardedEmployees), _uow.ConnectionName);
            }
        }

        private int[] GetKudosYearlyMultipliersArray(string multipliers)
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

        private int GetLoyaltyKudosEmploymentYear(DateTime employmentDate, DateTime loyaltyAddedDate)
        {
            var employmentYear = loyaltyAddedDate.Year - employmentDate.Year;
            if (loyaltyAddedDate < employmentDate.AddYears(employmentYear))
            {
                employmentYear = employmentYear >= 1 ? employmentYear - 1 : employmentYear;
            }

            return employmentYear;
        }
    }
}