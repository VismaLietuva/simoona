using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Domain.Services.Kudos
{
    public class KudosCalculatorSettingsService : IKudosCalculatorSettingsService
    {
        // The rates the Kudos committee's calculator spreadsheet used before they became editable.
        public static KudosCalculatorSettingsDto Defaults => new()
        {
            VatPercent = 21,
            TaxPercent = 43,
            TeamMultiplier = 1.5m,
            CreditPercent = 20,
            TechWearMonths = 36,
            FurnitureWearMonths = 72,
            BuyoutMinimum = 1
        };

        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<KudosCalculatorSettings> _settings;

        public KudosCalculatorSettingsService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _settings = uow.GetDbSet<KudosCalculatorSettings>();
        }

        public async Task<KudosCalculatorSettingsDto> GetAsync(UserAndOrganizationDto userOrg)
        {
            var settings = await _settings
                .AsNoTracking()
                .FirstOrDefaultAsync(row => row.OrganizationId == userOrg.OrganizationId);

            if (settings == null)
            {
                return Defaults;
            }

            return new KudosCalculatorSettingsDto
            {
                VatPercent = settings.VatPercent,
                TaxPercent = settings.TaxPercent,
                TeamMultiplier = settings.TeamMultiplier,
                CreditPercent = settings.CreditPercent,
                TechWearMonths = settings.TechWearMonths,
                FurnitureWearMonths = settings.FurnitureWearMonths,
                BuyoutMinimum = settings.BuyoutMinimum,
                GuideUrl = settings.GuideUrl,
                WhitelistUrl = settings.WhitelistUrl
            };
        }

        public async Task UpdateAsync(KudosCalculatorSettingsDto input, UserAndOrganizationDto userOrg)
        {
            Validate(input);

            var settings = await _settings.FirstOrDefaultAsync(row => row.OrganizationId == userOrg.OrganizationId);

            if (settings == null)
            {
                settings = new KudosCalculatorSettings { OrganizationId = userOrg.OrganizationId };
                _settings.Add(settings);
            }

            settings.VatPercent = input.VatPercent;
            settings.TaxPercent = input.TaxPercent;
            settings.TeamMultiplier = input.TeamMultiplier;
            settings.CreditPercent = input.CreditPercent;
            settings.TechWearMonths = input.TechWearMonths;
            settings.FurnitureWearMonths = input.FurnitureWearMonths;
            settings.BuyoutMinimum = input.BuyoutMinimum;
            settings.GuideUrl = NormalizeUrl(input.GuideUrl);
            settings.WhitelistUrl = NormalizeUrl(input.WhitelistUrl);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private static void Validate(KudosCalculatorSettingsDto input)
        {
            if (input == null)
            {
                throw Invalid("Settings are required");
            }

            RequireRange(input.VatPercent, 0, 100, nameof(input.VatPercent));
            RequireRange(input.TaxPercent, 0, 200, nameof(input.TaxPercent));
            RequireRange(input.TeamMultiplier, 1, 10, nameof(input.TeamMultiplier));
            RequireRange(input.CreditPercent, 0, 100, nameof(input.CreditPercent));
            RequireRange(input.TechWearMonths, 1, 240, nameof(input.TechWearMonths));
            RequireRange(input.FurnitureWearMonths, 1, 240, nameof(input.FurnitureWearMonths));
            RequireRange(input.BuyoutMinimum, 0, 1000, nameof(input.BuyoutMinimum));
            RequireUrl(input.GuideUrl, nameof(input.GuideUrl));
            RequireUrl(input.WhitelistUrl, nameof(input.WhitelistUrl));
        }

        private static void RequireRange(decimal value, decimal min, decimal max, string field)
        {
            if (value < min || value > max)
            {
                throw Invalid($"{field} must be between {min} and {max}");
            }
        }

        private static void RequireUrl(string value, string field)
        {
            var url = NormalizeUrl(value);

            if (url == null)
            {
                return;
            }

            var isWebUrl = Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                           (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

            if (!isWebUrl || url.Length > KudosCalculatorSettings.MaxUrlLength)
            {
                throw Invalid($"{field} must be an http(s) address of at most {KudosCalculatorSettings.MaxUrlLength} characters");
            }
        }

        private static string NormalizeUrl(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static ValidationException Invalid(string message)
        {
            return new ValidationException(ErrorCodes.KudosCalculatorSettingsInvalid, message);
        }
    }
}
