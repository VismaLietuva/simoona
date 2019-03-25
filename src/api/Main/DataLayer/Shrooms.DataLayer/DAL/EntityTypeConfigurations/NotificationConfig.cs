namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    using System.Data.Entity.ModelConfiguration;
    using Shrooms.EntityModels.Models.Notifications;

    internal class NotificationConfig : EntityTypeConfiguration<Notification>
    {
        public NotificationConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));
        }
    }
}
