using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Polls;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Polls
{
    internal class PollEntityConfig : IEntityTypeConfiguration<Poll>
    {
        public void Configure(EntityTypeBuilder<Poll> builder)
        {
            builder.ToTable("Polls");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(Poll.MaxTitleLength);
            builder.Property(x => x.Description).HasMaxLength(Poll.MaxDescriptionLength);
            builder.Property(x => x.ReviewReason).HasMaxLength(Poll.MaxReasonLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");
            builder.Property(x => x.Deadline).HasColumnType("datetime2");
            builder.Property(x => x.ClosedAt).HasColumnType("datetime2");
            builder.Property(x => x.ReviewedAt).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.OrganizationId, x.State })
                .HasDatabaseName("IX_Polls_OrganizationId_State");

            builder.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Wall)
                .WithMany()
                .HasForeignKey(x => x.WallId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReviewedBy)
                .WithMany()
                .HasForeignKey(x => x.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Questions)
                .WithOne(x => x.Poll)
                .HasForeignKey(x => x.PollId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
