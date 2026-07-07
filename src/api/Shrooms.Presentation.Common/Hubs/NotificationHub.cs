using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.ViewModels.Notifications;

namespace Shrooms.Presentation.Common.Hubs
{
    public class NotificationHub : BaseHub
    {
        private static readonly ConcurrentDictionary<UserAndOrganizationHubDto, HubUser> _notificationHubUsers =
            new ConcurrentDictionary<UserAndOrganizationHubDto, HubUser>();

        public static async Task SendWallNotificationAsync(IHubContext<NotificationHub> hubContext, int wallId, IEnumerable<string> membersIds, WallType wallType, UserAndOrganizationHubDto userOrg)
        {
            var connectionIds = _notificationHubUsers
                .Where(u => membersIds.Contains(u.Key.UserId) &&
                            u.Key.OrganizationName == userOrg.OrganizationName &&
                            u.Key.OrganizationId == userOrg.OrganizationId)
                .SelectMany(u => u.Value.ConnectionIds)
                .ToList();

            await hubContext.Clients.Clients(connectionIds).SendAsync("newContent", wallId, wallType);
        }

        public static async Task SendNotificationToAllUsersAsync(IHubContext<NotificationHub> hubContext, NotificationViewModel notification, UserAndOrganizationHubDto userOrg)
        {
            var connectionIds = _notificationHubUsers
                .Where(x => x.Key.UserId != userOrg.UserId &&
                            x.Key.OrganizationId == userOrg.OrganizationId &&
                            x.Key.OrganizationName == userOrg.OrganizationName)
                .SelectMany(u => u.Value.ConnectionIds)
                .ToList();

            await hubContext.Clients.Clients(connectionIds).SendAsync("newNotification", notification);
        }

        public static async Task SendNotificationToParticularUsersAsync(
            IHubContext<NotificationHub> hubContext,
            NotificationViewModel notification,
            UserAndOrganizationHubDto userOrg,
            IEnumerable<string> membersIds)
        {
            var connectionIds = _notificationHubUsers
                .Where(u => membersIds.Contains(u.Key.UserId) &&
                            u.Key.OrganizationId == userOrg.OrganizationId &&
                            u.Key.OrganizationName == userOrg.OrganizationName)
                .SelectMany(u => u.Value.ConnectionIds)
                .ToList();

            await hubContext.Clients.Clients(connectionIds).SendAsync("newNotification", notification);
        }

        public override Task OnConnectedAsync()
        {
            var userOrg = GetUserAndTenant();
            MapUserWithConnection(userOrg);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            var userOrg = GetUserAndTenant();
            RemoveUserConnections(userOrg);

            return base.OnDisconnectedAsync(exception);
        }

        private void RemoveUserConnections(UserAndOrganizationHubDto userOrg)
        {
            _notificationHubUsers.TryGetValue(userOrg, out var user);

            if (user == null)
            {
                return;
            }

            lock (user.ConnectionIds)
            {
                user.ConnectionIds.RemoveWhere(cid => cid.Equals(Context.ConnectionId));

                if (user.ConnectionIds.Any())
                {
                    return;
                }

                _notificationHubUsers.TryRemove(userOrg, out _);
            }
        }

        private void MapUserWithConnection(UserAndOrganizationHubDto userOrg)
        {
            var user = _notificationHubUsers.GetOrAdd(userOrg, _ => new HubUser
            {
                Id = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId,
                OrganizationName = userOrg.OrganizationName,
                ConnectionIds = new HashSet<string>()
            });

            lock (user.ConnectionIds)
            {
                user.ConnectionIds.Add(Context.ConnectionId);
            }
        }
    }
}
