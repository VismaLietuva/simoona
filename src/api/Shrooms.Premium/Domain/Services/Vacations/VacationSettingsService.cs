using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class VacationSettingsService : IVacationSettingsService
    {
        public const string DefaultOrderPrefix = "AT-";

        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<Organization> _organizationDbSet;
        private readonly DbSet<VacationOrder> _orderDbSet;

        public VacationSettingsService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _organizationDbSet = uow.GetDbSet<Organization>();
            _orderDbSet = uow.GetDbSet<VacationOrder>();
        }

        public async Task<VacationSettingsDto> GetAsync(UserAndOrganizationDto userOrg)
        {
            var organization = await _organizationDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == userOrg.OrganizationId);

            return await ToDtoAsync(organization, userOrg.OrganizationId);
        }

        public async Task<VacationSettingsDto> UpdateAsync(VacationSettingsDto settings, UserAndOrganizationDto userOrg)
        {
            var organization = await _organizationDbSet
                .FirstOrDefaultAsync(o => o.Id == userOrg.OrganizationId);

            if (organization == null)
            {
                return await ToDtoAsync(null, userOrg.OrganizationId);
            }

            organization.VacationOrderPrefix = NormalizePrefix(settings.OrderPrefix);

            // Clamp rather than reject: the client clamps identically, so an
            // out-of-bounds value only arrives from a hand-crafted request.
            organization.VacationOrderStartNumber = Clamp(
                settings.OrderStartNumber,
                VacationSettingLimits.MinOrderStartNumber,
                VacationSettingLimits.MaxOrderStartNumber);

            organization.VacationOrderLetterhead = Blank(settings.OrderLetterhead);
            organization.VacationOrderCity = Truncate(Blank(settings.OrderCity), 100);
            organization.VacationOrderSignature = Truncate(Blank(settings.OrderSignature), 200);

            await _uow.SaveChangesAsync(userOrg.UserId);

            return await ToDtoAsync(organization, userOrg.OrganizationId);
        }

        /// <summary>Nullable columns read as "unset", so each falls back rather than to zero or blank.</summary>
        internal static VacationSettingsDto Resolve(Organization organization)
        {
            return new VacationSettingsDto
            {
                OrderPrefix = string.IsNullOrWhiteSpace(organization?.VacationOrderPrefix)
                    ? DefaultOrderPrefix
                    : organization.VacationOrderPrefix,
                OrderStartNumber = organization?.VacationOrderStartNumber ?? VacationSettingLimits.MinOrderStartNumber,
                OrderLetterhead = organization?.VacationOrderLetterhead ?? organization?.Name,
                OrderCity = organization?.VacationOrderCity,
                OrderSignature = organization?.VacationOrderSignature
            };
        }

        private async Task<VacationSettingsDto> ToDtoAsync(Organization organization, int organizationId)
        {
            var dto = Resolve(organization);
            dto.NextOrderNumber = await NextOrderNumberAsync(_orderDbSet, organizationId, dto);
            return dto;
        }

        /// <summary>
        /// Never below the configured start, so lowering that setting cannot hand
        /// out a number already on a signed document.
        /// </summary>
        internal static async Task<int> NextOrderNumberAsync(
            DbSet<VacationOrder> orders,
            int organizationId,
            VacationSettingsDto settings)
        {
            var highest = await orders
                .AsNoTracking()
                .Where(order => order.OrganizationId == organizationId && order.Prefix == settings.OrderPrefix)
                .Select(order => (int?)order.Number)
                .MaxAsync();

            return Math.Max(settings.OrderStartNumber, (highest ?? 0) + 1);
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Min(max, Math.Max(min, value));
        }

        private static string Blank(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string Truncate(string value, int max)
        {
            return value != null && value.Length > max ? value.Substring(0, max) : value;
        }

        /// <summary>
        /// The prefix reaches a filename and a Content-Disposition header, so a
        /// safe alphabet keeps a quote or a line break out of both.
        /// </summary>
        private static string NormalizePrefix(string prefix)
        {
            var cleaned = new string((prefix ?? string.Empty)
                // No dots or slashes: the prefix ends up in a file name inside a
                // zip, where "../" would place the entry outside the folder.
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')
                .ToArray());

            if (cleaned.Length == 0)
            {
                return DefaultOrderPrefix;
            }

            return cleaned.Length > VacationOrder.MaxPrefixLength
                ? cleaned.Substring(0, VacationOrder.MaxPrefixLength)
                : cleaned;
        }
    }
}
