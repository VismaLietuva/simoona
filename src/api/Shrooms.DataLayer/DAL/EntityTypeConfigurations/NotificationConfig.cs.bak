using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models.Notifications;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class NotificationConfig : EntityTypeConfiguration<Notification>
    {
        public NotificationConfig()
        {
            Map(e => e.Requires("IsDeleted").HasValue(false));
        }
    }
}
