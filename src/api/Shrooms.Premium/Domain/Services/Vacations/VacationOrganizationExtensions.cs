using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    internal static class VacationOrganizationExtensions
    {
        public static async Task<string> TimeZoneAsync(this DbSet<Organization> organizations, int organizationId)
        {
            return await organizations
                .AsNoTracking()
                .Where(organization => organization.Id == organizationId)
                .Select(organization => organization.TimeZone)
                .FirstOrDefaultAsync();
        }

        /// <summary>Today in the organisation's zone — not UTC, which is a day behind late in the evening.</summary>
        public static async Task<DateTime> TodayAsync(this DbSet<Organization> organizations, int organizationId)
        {
            return VacationCalculator.TodayIn(await organizations.TimeZoneAsync(organizationId));
        }

        /// <summary>How far the organisation's zone is ahead of UTC right now.</summary>
        public static async Task<TimeSpan> UtcOffsetAsync(this DbSet<Organization> organizations, int organizationId)
        {
            return VacationCalculator.NowIn(await organizations.TimeZoneAsync(organizationId)) - DateTime.UtcNow;
        }
    }
}
