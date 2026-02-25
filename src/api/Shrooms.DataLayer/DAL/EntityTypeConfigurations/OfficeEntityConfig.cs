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
        }
    }
}
