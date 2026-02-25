using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class WallMembersConfiguration : IEntityTypeConfiguration<WallMember>
    {
        public void Configure(EntityTypeBuilder<WallMember> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
