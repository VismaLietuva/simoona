using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public class LoyaltyKudosCalculator : ILoyaltyKudosCalculator
    {
        private const string LoyaltyKudosBotName = "KudosLoyaltyBot";

        public KudosLog CreateLoyaltyKudosLog(ApplicationUser recipient, KudosType loyaltyKudosType, int organizationId, int[] kudosYearlyMultipliers, int yearOfEmployment)
        {
            if (yearOfEmployment <= 0)
            {
                throw new ArgumentException("Invalid argument", nameof(yearOfEmployment));
            }

            if (organizationId <= 0)
            {
                throw new ArgumentException("Invalid argument", nameof(organizationId));
            }

            if (recipient == null)
            {
                throw new ArgumentNullException(nameof(recipient));
            }

            if (loyaltyKudosType == null)
            {
                throw new ArgumentNullException(nameof(loyaltyKudosType));
            }

            if (kudosYearlyMultipliers == null || kudosYearlyMultipliers.Length == 0)
            {
                throw new ArgumentException("Invalid argument", nameof(kudosYearlyMultipliers));
            }

            var timestamp = DateTime.UtcNow;
            var yearlyLoyaltyKudosMultiplier = CalculateYearlyMultiplier(yearOfEmployment, kudosYearlyMultipliers);
            if (yearlyLoyaltyKudosMultiplier == 0)
            {
                return null;
            }

            var loyaltyLog = new KudosLog
            {
                Created = recipient.EmploymentDate.Value.AddYears(yearOfEmployment),
                Modified = timestamp,
                CreatedBy = LoyaltyKudosBotName,
                Comments = CreateLoyaltyKudosComment(yearOfEmployment),
                EmployeeId = recipient.Id,
                KudosTypeName = loyaltyKudosType.Name,
                KudosTypeValue = loyaltyKudosType.Value,
                MultiplyBy = yearlyLoyaltyKudosMultiplier,
                Points = yearlyLoyaltyKudosMultiplier * loyaltyKudosType.Value,
                Status = KudosStatus.Approved,
                OrganizationId = organizationId
            };

            recipient.ReceiveKudos(loyaltyLog);
            return loyaltyLog;
        }

        public IEnumerable<int> CalculateYearsToAwardFor(int yearsEmployed, int loyaltyAwardsAlreadyReceived)
        {
            var yearsToAwardCount = yearsEmployed - loyaltyAwardsAlreadyReceived;
            var yearsToAwardFor = Enumerable.Empty<int>();
            if (yearsToAwardCount >= 0)
            {
                yearsToAwardFor = Enumerable.Range(loyaltyAwardsAlreadyReceived + 1, yearsToAwardCount);
            }

            return yearsToAwardFor;
        }

        public List<KudosLog> GetEmployeeLoyaltyKudosLog(EmployeeLoyaltyKudosDTO employeeLoyaltyKudos,
                                                         KudosType loyaltyType,
                                                         int organizationId,
                                                         int[] kudosYearlyMultipliers)
        {
            var receivedLoyaltiesCount = employeeLoyaltyKudos.AwardedLoyaltyKudosCount;
            var yearsToAwardFor = CalculateYearsToAwardFor(employeeLoyaltyKudos.Employee.YearsEmployed, receivedLoyaltiesCount);
            var loyaltyKudosLogList = new List<KudosLog>();

            foreach (var year in yearsToAwardFor)
            {
                if (employeeLoyaltyKudos.AwardedEmploymentYears.Any(y => y == year))
                {
                    continue;
                }

                var loyaltyKudosLog = CreateLoyaltyKudosLog(employeeLoyaltyKudos.Employee, loyaltyType, organizationId, kudosYearlyMultipliers, year);
                if (loyaltyKudosLog != null)
                {
                    loyaltyKudosLogList.Add(loyaltyKudosLog);
                }
            }

            return loyaltyKudosLogList;
        }

        private static string CreateLoyaltyKudosComment(int yearAwardedFor)
        {
            return $"Kudos for {yearAwardedFor} year loyalty";
        }

        private static int CalculateYearlyMultiplier(int yearOfEmployment, IReadOnlyList<int> multipliers)
        {
            return yearOfEmployment <= multipliers.Count ? multipliers[yearOfEmployment - 1] : multipliers.Last();
        }
    }
}