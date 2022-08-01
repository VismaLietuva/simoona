using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.User
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

        public bool MentionEmailNotifications;

        public bool CreatedLotteryEmailNotifications;

        public IEnumerable<WallNotificationsViewModel> Walls;
    }
}
