using System;
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
        public const string ConcurrentSaveMessage = "These settings were just saved by another request. Reload the page and try again.";

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
            catch (DbUpdateException e) when (e.IsUserSettingsKeyViolation())
            {
                throw ConcurrentSave();
            }
        }

        public static ValidationException ConcurrentSave() => new(ErrorCodes.ConcurrencyError, ConcurrentSaveMessage);

        /// <summary>
        /// True only for a duplicate UserSettings row. The same save can carry other
        /// inserts (confirming a user adds wall memberships and kudos); a duplicate
        /// there is a different problem and is left to surface as itself.
        /// </summary>
        public static bool IsUserSettingsKeyViolation(this DbUpdateException e)
        {
            // 2627: primary key or unique constraint violated. The message names the constraint.
            return e.InnerException is SqlException { Number: 2627 } sql
                   && sql.Message.Contains(PrimaryKeyName, StringComparison.OrdinalIgnoreCase);
        }

        // The name AddUserSettings gives the key.
        private const string PrimaryKeyName = "PK_UserSettings";
    }
}
