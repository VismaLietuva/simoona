using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Group;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class GroupEntityConfig : IEntityTypeConfiguration<Group>
    {
        internal const int UserIdLength = 128;

        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Members carry their own dates and roles, so the relationship is an entity
            // rather than a plain join table - see GroupMemberEntityConfig.
            builder.HasMany(g => g.Members)
                .WithOne(m => m.Group)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(g => g.References)
                .WithOne(r => r.Group)
                .HasForeignKey(r => r.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(g => g.Description).HasMaxLength(5000);

            builder.Ignore(g => g.IsPending);

            builder.HasOne(g => g.Organization)
                .WithMany()
                .HasForeignKey(g => g.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Name is the handle a group is tagged by in posts, so it is indexed for lookup.
            builder.HasIndex(g => new { g.OrganizationId, g.Name });
        }
    }
}
