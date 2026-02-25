using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class FilterPresetEntityConfig : IEntityTypeConfiguration<FilterPreset>
    {
        public void Configure(EntityTypeBuilder<FilterPreset> builder)
        {
            builder.HasQueryFilter(filter => !filter.IsDeleted);

            builder.Property(filter => filter.Preset)
                .IsRequired();

            builder.Property(filter => filter.Name)
                .IsRequired();

            builder.Property(filter => filter.ForPage)
                .IsRequired();
        }
    }
}
