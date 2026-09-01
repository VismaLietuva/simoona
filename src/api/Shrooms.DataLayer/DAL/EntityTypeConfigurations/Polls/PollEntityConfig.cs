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

            // ClientNoAction, not Restrict: the database keeps NO ACTION either way, but Restrict makes
            // EF enforce the required relationship on the client. Because polls and walls are soft deleted,
            // the poll stays tracked with a non-nullable WallId when the wall is removed, and EF's cascade
            // pass throws "the association ... has been severed" before SaveChanges is reached.
            builder.HasOne(x => x.Wall)
                .WithMany()
                .HasForeignKey(x => x.WallId)
                .OnDelete(DeleteBehavior.ClientNoAction);

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
