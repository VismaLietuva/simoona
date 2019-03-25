using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Users
{
    public class UserNotificationsSettingsDto
    {
        public bool EventsAppNotifications;

        public bool EventsEmailNotifications;

        public bool ProjectsAppNotifications;

        public bool ProjectsEmailNotifications;

        public bool MyPostsAppNotifications;

        public bool MyPostsEmailNotifications;

        public bool FollowingPostsAppNotifications;

        public bool FollowingPostsEmailNotifications;

        public IEnumerable<WallNotificationsDto> Walls;
    }
}
