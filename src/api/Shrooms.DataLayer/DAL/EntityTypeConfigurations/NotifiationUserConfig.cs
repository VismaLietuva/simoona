using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Notifications;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class NotifiationUserConfig : IEntityTypeConfiguration<NotificationUser>
    {
        public void Configure(EntityTypeBuilder<NotificationUser> builder)
        {
            builder.ToTable("NotificationUsers");

            builder.HasKey(x => new { x.NotificationId, x.UserId });

            builder.HasIndex(x => x.IsAlreadySeen)
                .HasDatabaseName("ix_notification_IsAlreadySeen");
        }
    }
}
