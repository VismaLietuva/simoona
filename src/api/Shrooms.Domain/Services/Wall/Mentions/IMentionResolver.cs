using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Domain.Services.Wall.Mentions
{
    public interface IMentionResolver
    {
        /// <summary>
        /// The users to notify about the mentions in <paramref name="messageBody"/>.
        /// Groups expand to the members active today, ids from another organization
        /// or from a group that may not be tagged are dropped, and the author never
        /// notifies themselves.
        /// </summary>
        /// <param name="legacyUserIds">
        /// Ids sent by a client that does not write tokens (the AngularJS app). Used
        /// only when the body carries no tokens at all, and validated the same way.
        /// </param>
        Task<IEnumerable<string>> ResolveAsync(
            string messageBody,
            IEnumerable<string> legacyUserIds,
            UserAndOrganizationDto userOrg);

        /// <summary>
        /// The users to notify after an edit: as <see cref="ResolveAsync"/>, minus
        /// everyone the previous body already mentioned. Re-saving a post must not
        /// mail the same people a second time.
        /// </summary>
        Task<IEnumerable<string>> ResolveAddedAsync(
            string messageBody,
            string previousMessageBody,
            IEnumerable<string> legacyUserIds,
            UserAndOrganizationDto userOrg);
    }
}
