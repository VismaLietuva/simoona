using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public class LoyaltyKudos
    {
        private const string LoyaltyKudosBotName = "KudosLoyaltyBot";

        private static string LoyaltyKudosComment(int yearAwardedFor) => $"Kudos for {yearAwardedFor} year loyalty";

        private LoyaltyKudos()
        { }

        public static KudosLog CreateLoyaltyKudosLog(ApplicationUser recipient, KudosType loyaltyKudosType, int organizationId, int[] kudosYearlyMultipliers, int yearOfEmployment)
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
                Created = timestamp,
                Modified = timestamp,
                CreatedBy = LoyaltyKudosBotName,
                Comments = LoyaltyKudosComment(yearOfEmployment),
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

        public static IEnumerable<int> CalculateYearsToAwardFor(int yearsEmployed, int loyaltyAwardsAlreadyReceived)
        {
            var yearsToAwardCount = yearsEmployed - loyaltyAwardsAlreadyReceived;
            var yearsToAwardFor = Enumerable.Empty<int>();
            if (yearsToAwardCount >= 0)
            {
                yearsToAwardFor = Enumerable.Range(loyaltyAwardsAlreadyReceived + 1, yearsToAwardCount);
            }

            return yearsToAwardFor;
        }

        private static int CalculateYearlyMultiplier(int yearOfEmployment, IReadOnlyList<int> multipliers)
        {
            return yearOfEmployment <= multipliers.Count ? multipliers[yearOfEmployment - 1] : multipliers.Last();
        }
    }
}
