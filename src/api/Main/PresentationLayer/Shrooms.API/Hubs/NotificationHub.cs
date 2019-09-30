using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.WebViewModels.Models.Notifications;

namespace Shrooms.API.Hubs
{
    [HubName("Notification")]
    public class NotificationHub : BaseHub
    {
        private static readonly ConcurrentDictionary<UserAndOrganizationHubDto, HubUser> NotificationHubUsers =
            new ConcurrentDictionary<UserAndOrganizationHubDto, HubUser>();

        /// <summary>
        /// Triggers New Content Available toolbox
        /// </summary>
        /// <param name="wallId"></param>
        /// <param name="membersIds"></param>
        /// <param name="wallType"></param>
        /// <param name="userOrg"></param>
        public static void SendWallNotification(int wallId, IEnumerable<string> membersIds, WallType wallType, UserAndOrganizationHubDto userOrg)
        {
            var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            var connectionIds = NotificationHubUsers
                .Where(u => membersIds.Contains(u.Key.UserId) &&
                            u.Key.OrganizationName == userOrg.OrganizationName &&
                            u.Key.OrganizationId == userOrg.OrganizationId)
                .SelectMany(u => u.Value.ConnectionIds)
                .ToList();

            notificationHub.Clients.Clients(connectionIds).newContent(wallId, wallType);
        }

        public static void SendNotificationToAllUsers(NotificationViewModel notification, UserAndOrganizationHubDto userOrg)
        {
            var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            var connectionIds = NotificationHubUsers
                .Where(x => x.Key.UserId != userOrg.UserId &&
                            x.Key.OrganizationId == userOrg.OrganizationId &&
                            x.Key.OrganizationName == userOrg.OrganizationName)
                .SelectMany(u => u.Value.ConnectionIds)
                .ToList();

            notificationHub.Clients.Clients(connectionIds).newNotification(notification);
        }

        public static void SendNotificationToParticularUsers(NotificationViewModel notification, UserAndOrganizationHubDto userOrg, string memberId)
        {
            SendNotificationToParticularUsers(notification, userOrg, new List<string> { memberId });
        }

        public static void SendNotificationToParticularUsers(NotificationViewModel notification, UserAndOrganizationHubDto userOrg, IEnumerable<string> membersIds)
        {
            var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            var connectionIds = NotificationHubUsers
                .Where(u => membersIds.Contains(u.Key.UserId) &&
                            u.Key.OrganizationId == userOrg.OrganizationId &&
                            u.Key.OrganizationName == userOrg.OrganizationName)
                .SelectMany(u => u.Value.ConnectionIds)
                .ToList();

            notificationHub.Clients.Clients(connectionIds).newNotification(notification);
        }

        public override Task OnConnected()
        {
            var userOrg = GetUserAndTenant();
            MapUserWithConnection(userOrg);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userOrg = GetUserAndTenant();
            RemoveUserConnections(userOrg);

            return base.OnDisconnected(stopCalled);
        }

        private void RemoveUserConnections(UserAndOrganizationHubDto userOrg)
        {
            NotificationHubUsers.TryGetValue(userOrg, out var user);

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

                NotificationHubUsers.TryRemove(userOrg, out _);
            }
        }

        private void MapUserWithConnection(UserAndOrganizationHubDto userOrg)
        {
            var user = NotificationHubUsers.GetOrAdd(userOrg, _ => new HubUser
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