using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class KudosBasketEntityConfig : IEntityTypeConfiguration<KudosBasket>
    {
        public void Configure(EntityTypeBuilder<KudosBasket> builder)
        {
            builder.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(25);

            builder.Property(b => b.Description)
                .HasMaxLength(5000);

            builder.HasQueryFilter(m => !m.IsDeleted);

            builder.HasOne(k => k.Organization)
                .WithMany()
                .HasForeignKey(k => k.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
