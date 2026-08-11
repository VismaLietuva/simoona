using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Group;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class GroupMemberEntityConfig : IEntityTypeConfiguration<GroupMember>
    {
        public void Configure(EntityTypeBuilder<GroupMember> builder)
        {
            builder.ToTable("GroupMembers");

            // Surrogate key: one person can hold several memberships of the same group.
            builder.HasKey(m => m.Id);

            builder.HasIndex(m => new { m.GroupId, m.UserId });

            builder.Property(m => m.Description).HasMaxLength(1000);

            builder.Property(m => m.UserId).IsRequired();

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
