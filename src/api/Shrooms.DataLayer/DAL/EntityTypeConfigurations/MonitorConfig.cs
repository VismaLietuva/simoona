using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class MonitorConfig : IEntityTypeConfiguration<EntityModels.Models.Monitors.Monitor>
    {
        public void Configure(EntityTypeBuilder<EntityModels.Models.Monitors.Monitor> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.HasOne(x => x.Organization)
              .WithMany()
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
