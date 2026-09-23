using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Group;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class GroupTypeEntityConfig : IEntityTypeConfiguration<GroupType>
    {
        public void Configure(EntityTypeBuilder<GroupType> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(t => t.AwardTemplate).HasMaxLength(500);

            builder.HasMany(t => t.Groups)
                .WithOne(g => g.GroupType)
                .HasForeignKey(g => g.GroupTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.KudosType)
                .WithMany()
                .HasForeignKey(t => t.KudosTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Organization)
                .WithMany()
                .HasForeignKey(t => t.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
