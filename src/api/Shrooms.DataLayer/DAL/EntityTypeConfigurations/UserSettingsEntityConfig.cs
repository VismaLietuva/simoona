using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class UserSettingsEntityConfig : IEntityTypeConfiguration<UserSettings>
    {
        public void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            builder.ToTable("UserSettings");

            // Shared primary key: the row is the user's, one per user.
            builder.HasKey(x => x.UserId);

            builder.Property(x => x.UserId).HasMaxLength(128);
            builder.Property(x => x.CultureCode).HasMaxLength(UserSettings.MaxCultureCodeLength);
            builder.Property(x => x.TimeZone).HasMaxLength(UserSettings.MaxTimeZoneLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasOne(x => x.User)
                .WithOne(x => x.Settings)
                .HasForeignKey<UserSettings>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
