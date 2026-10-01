using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.UserPreferences
{
    /// <summary>
    /// The sidebar items a user has starred, as a JSON array of navigation item
    /// keys in display order. Stored as a section of the user's preferences
    /// document, next to the wall widget layout.
    /// </summary>
    public class SidebarFavoritesService : ISidebarFavoritesService
    {
        public const int MaxFavoritesLength = 4096;

        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<ApplicationUser> _usersDbSet;

        public SidebarFavoritesService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public async Task<string> GetAsync(UserAndOrganizationDto userOrg)
        {
            var stored = await _usersDbSet
                .Where(user => user.Id == userOrg.UserId && user.OrganizationId == userOrg.OrganizationId)
                .Select(user => user.WallWidgetPreferences)
                .FirstOrDefaultAsync();

            return UserPreferencesDocument.GetSection(stored, UserPreferencesDocument.SidebarFavoritesSection);
        }

        public async Task SaveAsync(string favorites, UserAndOrganizationDto userOrg)
        {
            ValidateFavorites(favorites);

            var user = await _usersDbSet
                .FirstOrDefaultAsync(u => u.Id == userOrg.UserId && u.OrganizationId == userOrg.OrganizationId);

            if (user == null)
            {
                throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
            }

            user.WallWidgetPreferences = UserPreferencesDocument.SetSection(
                user.WallWidgetPreferences,
                UserPreferencesDocument.SidebarFavoritesSection,
                favorites);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        // Which keys are real navigation items is the client's business (it drops
        // unknown ones on read); the server only guarantees the shape.
        private static void ValidateFavorites(string favorites)
        {
            if (string.IsNullOrWhiteSpace(favorites) || favorites.Length > MaxFavoritesLength)
            {
                throw InvalidFavorites();
            }

            JsonDocument document;

            try
            {
                document = JsonDocument.Parse(favorites);
            }
            catch (JsonException)
            {
                throw InvalidFavorites();
            }

            using (document)
            {
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Array ||
                    root.EnumerateArray().Any(item => item.ValueKind != JsonValueKind.String))
                {
                    throw InvalidFavorites();
                }
            }
        }

        private static ValidationException InvalidFavorites()
        {
            return new ValidationException(ErrorCodes.SidebarFavoritesInvalid, "Invalid sidebar favorites");
        }
    }
}
