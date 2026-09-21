using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Wall.Widgets
{
    public class WallWidgetPreferencesService : IWallWidgetPreferencesService
    {
        public const int MaxPreferencesLength = 4096;

        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<ApplicationUser> _usersDbSet;

        public WallWidgetPreferencesService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public async Task<string> GetAsync(UserAndOrganizationDto userOrg)
        {
            return await _usersDbSet
                .Where(user => user.Id == userOrg.UserId && user.OrganizationId == userOrg.OrganizationId)
                .Select(user => user.WallWidgetPreferences)
                .FirstOrDefaultAsync();
        }

        public async Task SaveAsync(string preferences, UserAndOrganizationDto userOrg)
        {
            ValidatePreferences(preferences);

            var user = await _usersDbSet
                .FirstOrDefaultAsync(u => u.Id == userOrg.UserId && u.OrganizationId == userOrg.OrganizationId);

            if (user == null)
            {
                throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
            }

            user.WallWidgetPreferences = preferences;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private static void ValidatePreferences(string preferences)
        {
            if (string.IsNullOrWhiteSpace(preferences) || preferences.Length > MaxPreferencesLength)
            {
                throw new ValidationException(ErrorCodes.WallWidgetPreferencesInvalid, "Invalid wall widget preferences");
            }

            JsonDocument document;

            try
            {
                document = JsonDocument.Parse(preferences);
            }
            catch (JsonException)
            {
                throw new ValidationException(ErrorCodes.WallWidgetPreferencesInvalid, "Invalid wall widget preferences");
            }

            using (document)
            {
                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    throw new ValidationException(ErrorCodes.WallWidgetPreferencesInvalid, "Invalid wall widget preferences");
                }
            }
        }
    }
}
