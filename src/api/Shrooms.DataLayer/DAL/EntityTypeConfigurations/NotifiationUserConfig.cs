using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models.Notifications;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class NotifiationUserConfig : EntityTypeConfiguration<NotificationUser>
    {
        public NotifiationUserConfig()
        {
            HasKey(x => new { x.NotificationId, x.UserId });

            Property(x => x.IsAlreadySeen)
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("ix_notification_IsAlreadySeen")));
        }
    }
}
