using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Group;

namespace Shrooms.Domain.Services.Wall.Mentions
{
    /// <summary>
    /// Groups live in Shrooms.Premium, which this project cannot reference - but the
    /// group entities are in the core data layer, so they are queried directly here
    /// rather than through IGroupsService.
    /// </summary>
    public class MentionResolver : IMentionResolver
    {
        /// <summary>
        /// Past this many tokens a body is spam rather than a conversation, and the
        /// people they expand to are capped too: every recipient costs one email sent
        /// one at a time, so an uncapped body could mail the whole organization.
        /// </summary>
        private const int MaxTokens = 20;

        private const int MaxRecipients = 500;

        private readonly ISystemClock _systemClock;
        private readonly DbSet<ApplicationUser> _usersDbSet;
        private readonly DbSet<Group> _groupsDbSet;

        public MentionResolver(IUnitOfWork2 uow, ISystemClock systemClock)
        {
            _systemClock = systemClock;
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _groupsDbSet = uow.GetDbSet<Group>();
        }

        public Task<IEnumerable<string>> ResolveAsync(
            string messageBody,
            IEnumerable<string> legacyUserIds,
            UserAndOrganizationDto userOrg)
        {
            return ResolveAddedAsync(messageBody, null, legacyUserIds, userOrg);
        }

        public async Task<IEnumerable<string>> ResolveAddedAsync(
            string messageBody,
            string previousMessageBody,
            IEnumerable<string> legacyUserIds,
            UserAndOrganizationDto userOrg)
        {
            var tokens = MentionTokenParser.Parse(messageBody);

            if (tokens.Count == 0)
            {
                return (await ExistingUserIdsAsync(legacyUserIds, userOrg)).Take(MaxRecipients);
            }

            var alreadyMentioned = MentionTokenParser.Parse(previousMessageBody);

            var added = tokens
                .Where(token => !alreadyMentioned.Any(previous => IsSameTarget(previous, token)))
                .Take(MaxTokens)
                .ToList();

            var userIds = added
                .Where(token => token.Kind == MentionKind.User)
                .Select(token => token.UserId);

            var resolved = new HashSet<string>(await ExistingUserIdsAsync(userIds, userOrg));

            var groupIds = added
                .Where(token => token.Kind == MentionKind.Group)
                .Select(token => token.GroupId)
                .Distinct()
                .ToList();

            if (groupIds.Any())
            {
                foreach (var memberId in await GroupMemberIdsAsync(groupIds, userOrg.OrganizationId))
                {
                    resolved.Add(memberId);
                }
            }

            resolved.Remove(userOrg.UserId);

            return resolved.Take(MaxRecipients);
        }

        private static bool IsSameTarget(MentionToken left, MentionToken right)
        {
            return left.Kind == right.Kind &&
                   (left.Kind == MentionKind.User
                       ? string.Equals(left.UserId, right.UserId, StringComparison.OrdinalIgnoreCase)
                       : left.GroupId == right.GroupId);
        }

        private async Task<IEnumerable<string>> ExistingUserIdsAsync(
            IEnumerable<string> userIds,
            UserAndOrganizationDto userOrg)
        {
            var ids = userIds?
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (ids == null || !ids.Any())
            {
                return Enumerable.Empty<string>();
            }

            var existing = await _usersDbSet
                .Where(user => ids.Contains(user.Id) && user.OrganizationId == userOrg.OrganizationId)
                .Select(user => user.Id)
                .ToListAsync();

            return existing.Where(id => id != userOrg.UserId);
        }

        private async Task<IEnumerable<string>> GroupMemberIdsAsync(ICollection<int> groupIds, int organizationId)
        {
            var utcNow = _systemClock.UtcNow;
            var today = utcNow.Date;

            // The date checks spell out GroupMember.IsActiveDuring rather than calling
            // it, so the whole thing stays one query. The comparison is against the
            // date, not the instant: the dates are captured as date-only, so a
            // timestamp comparison would drop a membership on its last day.
            return await _groupsDbSet
                .Where(group => groupIds.Contains(group.Id) &&
                                group.OrganizationId == organizationId &&
                                group.Status == GroupStatus.Approved &&
                                group.GroupType.HasGroupTag)
                .SelectMany(group => group.Members)
                .Where(member => (member.StartDate == null || member.StartDate <= today) &&
                                 (member.EndDate == null || member.EndDate >= today))
                .Select(member => member.UserId)
                .Distinct()
                .ToListAsync();
        }
    }
}
