using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class WallModeratorsConfiguration : IEntityTypeConfiguration<WallModerator>
    {
        public void Configure(EntityTypeBuilder<WallModerator> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}