using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class WallConfiguration : IEntityTypeConfiguration<Wall>
    {
        public void Configure(EntityTypeBuilder<Wall> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(x => x.Organization)
               .WithMany()
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
