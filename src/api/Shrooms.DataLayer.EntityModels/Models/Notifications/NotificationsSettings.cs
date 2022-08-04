namespace Shrooms.DataLayer.EntityModels.Models.Notifications
{
    public class NotificationsSettings : BaseModelWithOrg
    {
        public bool EventsAppNotifications { get; set; }

        public bool EventsEmailNotifications { get; set; }

        public bool ProjectsAppNotifications { get; set; }

        public bool ProjectsEmailNotifications { get; set; }

        public bool MyPostsAppNotifications { get; set; }

        public bool MyPostsEmailNotifications { get; set; }

        public bool FollowingPostsAppNotifications { get; set; }

        public bool FollowingPostsEmailNotifications { get; set; }

        public bool EventWeeklyReminderAppNotifications { get; set; }

        public bool EventWeeklyReminderEmailNotifications { get; set; }

        public bool MentionEmailNotifications { get; set; }

        public bool CreatedLotteryEmailNotifications { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
