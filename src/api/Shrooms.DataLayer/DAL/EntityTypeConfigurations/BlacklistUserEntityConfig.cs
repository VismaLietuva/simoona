using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class BlacklistUserEntityConfig : IEntityTypeConfiguration<BlacklistUser>
    {
        public void Configure(EntityTypeBuilder<BlacklistUser> builder)
        {
            builder.Ignore(u => u.IsDeleted);

            builder.HasOne(u => u.ModifiedByUser)
                .WithMany()
                .HasForeignKey(u => u.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.CreatedByUser)
                .WithMany()
                .HasForeignKey(u => u.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(u => u.Reason)
                .IsRequired(false);

            builder.Property(u => u.UserId)
                .IsRequired();

            builder.Property(u => u.EndDate)
                .IsRequired();

            builder.Property(u => u.Status)
                .IsRequired();

            builder.Property(u => u.ModifiedBy)
                .IsRequired();
        }
    }
}
