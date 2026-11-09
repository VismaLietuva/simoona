using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;

namespace Shrooms.Domain.Extensions
{
    public static class UserSettingsSaveExtensions
    {
        /// <summary>
        /// Saves a change made through <c>ApplicationUser.GetOrCreateSettings()</c>.
        /// When the user had no settings row, two parallel first writes both insert
        /// one with the same key and the second fails on the primary key. That
        /// request gets a concurrency error to retry instead of a 500.
        /// </summary>
        public static async Task SaveUserSettingsAsync(this IUnitOfWork2 uow, string userId)
        {
            try
            {
                await uow.SaveChangesAsync(userId);
            }
            catch (DbUpdateException e) when (IsUniqueViolation(e))
            {
                throw new ValidationException(ErrorCodes.ConcurrencyError, "Your settings were just saved by another request. Reload the page and try again.");
            }
        }

        private static bool IsUniqueViolation(DbUpdateException e)
        {
            return e.InnerException is SqlException sql
                   && (sql.Number == 2601 || sql.Number == 2627);
        }
    }
}
