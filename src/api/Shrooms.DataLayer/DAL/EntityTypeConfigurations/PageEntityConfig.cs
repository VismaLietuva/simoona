using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class PageEntityConfig : IEntityTypeConfiguration<Page>
    {
        public void Configure(EntityTypeBuilder<Page> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(e => e.ParentPage)
                .WithMany()
                .IsRequired(false);

            builder.HasOne(p => p.Organization)
                .WithMany()
                .HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
