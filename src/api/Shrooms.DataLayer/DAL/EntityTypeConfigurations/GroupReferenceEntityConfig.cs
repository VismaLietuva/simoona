using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Group;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class GroupReferenceEntityConfig : IEntityTypeConfiguration<GroupReference>
    {
        public void Configure(EntityTypeBuilder<GroupReference> builder)
        {
            builder.ToTable("GroupReferences");

            builder.Property(r => r.Url).HasMaxLength(500).IsRequired();

            builder.Property(r => r.Name).HasMaxLength(100);

            builder.HasIndex(r => r.GroupId);
        }
    }
}
