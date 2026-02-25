using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class OfficeEntityConfig : IEntityTypeConfiguration<Office>
    {
        public void Configure(EntityTypeBuilder<Office> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(o => o.Organization)
                .WithMany()
                .HasForeignKey(o => o.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsOne(o => o.Address, a =>
            {
                a.Property(x => x.Country).HasColumnName("Country");
                a.Property(x => x.City).HasColumnName("City");
                a.Property(x => x.Street).HasColumnName("Street");
                a.Property(x => x.Building).HasColumnName("Building");
            });
        }
    }
}
