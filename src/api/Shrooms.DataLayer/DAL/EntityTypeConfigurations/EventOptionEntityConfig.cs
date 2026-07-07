using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class EventOptionEntityConfig : IEntityTypeConfiguration<EventOption>
    {
        public void Configure(EntityTypeBuilder<EventOption> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.Option)
                .IsRequired();
        }
    }
}
