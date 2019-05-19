using System;
using System.Data.Entity;
using System.Linq;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Kudos;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public class LoyaltyKudosService : ILoyaltyKudosService
    {
        private const string LoyaltyKudosTypeName = "Loyalty";
        private static object concurrencyLock = new object();

        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<KudosLog> _kudosLogsDbSet;
        private readonly IDbSet<KudosType> _kudosTypesDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<Organization> _organizationsDbSet;

        private readonly ILogger _logger;
        private readonly IKudosPremiumNotificationService _kudosNotificationService;

        public LoyaltyKudosService(IUnitOfWork2 uow, ILogger logger, IKudosPremiumNotificationService kudosNotificationService)
        {
            _logger = logger;
            _kudosNotificationService = kudosNotificationService;

            _uow = uow;
            _kudosLogsDbSet = uow.GetDbSet<KudosLog>();
            _kudosTypesDbSet = uow.GetDbSet<KudosType>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
        }

        public void AwardEmployeesWithKudos(string organizationName)
        {
            lock (concurrencyLock)
            {
                if (string.IsNullOrEmpty(organizationName))
                {
                    throw new ArgumentNullException(organizationName);
                }

                var organization = _organizationsDbSet
                    .Where(o => o.ShortName == organizationName)
                    .Select(o => new
                    {
                        o.Id,
                        o.KudosYearlyMultipliers
                    })
                    .SingleOrDefault();

                var organizationId = organization.Id;
                var kudosYearlyMultipliers = ParseKudosYearlyMultipliersString(organization.KudosYearlyMultipliers);
                if (kudosYearlyMultipliers == null)
                {
                    return;
                }

                var loyaltyType = _kudosTypesDbSet
                    .SingleOrDefault(t => t.Name == LoyaltyKudosTypeName);

                var employees = _usersDbSet
                    .Where(u =>
                        u.OrganizationId == organizationId &&
                        u.EmploymentDate.HasValue)
                    .ToList();

                var employeesIds = employees.Select(x => x.Id).ToList();
                var employeesReceivedLoyaltyKudos = _kudosLogsDbSet
                    .Where(l => l.OrganizationId == organizationId && employeesIds.Contains(l.EmployeeId))
                    .GroupBy(x => x.EmployeeId)
                    .Select(l => new
                    {
                        Id = l.Key,
                        loyaltiesReceivedAlready = l.Count(log => log.Status == KudosStatus.Approved && log.KudosTypeName == LoyaltyKudosTypeName)
                    })
                    .ToList();

                foreach (var employee in employees)
                {
                    var loyaltiesAlreadyReceived = employeesReceivedLoyaltyKudos.FirstOrDefault(x => x.Id == employee.Id)?.loyaltiesReceivedAlready ?? 0;
                    var yearsToAwardFor = LoyaltyKudos.CalculateYearsToAwardFor(employee.YearsEmployed, loyaltiesAlreadyReceived);

                    try
                    {
                        foreach (var year in yearsToAwardFor)
                        {
                            var loyaltyKudosLog = LoyaltyKudos.CreateLoyaltyKudosLog(employee, loyaltyType, organizationId, kudosYearlyMultipliers, year);
                            if (loyaltyKudosLog != null)
                            {
                                _kudosLogsDbSet.Add(loyaltyKudosLog);
                                _kudosNotificationService.SendLoyaltyBotNotification(loyaltyKudosLog);
                            }
                        }
                    }
                    catch (ArgumentException e)
                    {
                        _logger.Error(e);
                    }
                }

                _uow.SaveChanges(false);
            }
        }

        private int[] ParseKudosYearlyMultipliersString(string multipliers)
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
    }
}