using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.WebViewModels.Models.User
{
    public class UserNotificationsSettingsViewModel
    {
        public bool EventsAppNotifications;

        public bool EventsEmailNotifications;

        public bool EventWeeklyReminderAppNotifications;

        public bool EventWeeklyReminderEmailNotifications;

        public bool ProjectsAppNotifications;

        public bool ProjectsEmailNotifications;

        public bool MyPostsAppNotifications;

        public bool MyPostsEmailNotifications;

        public bool FollowingPostsAppNotifications;

        public bool FollowingPostsEmailNotifications;

        public IEnumerable<WallNotificationsViewModel> Walls;
    }
}
