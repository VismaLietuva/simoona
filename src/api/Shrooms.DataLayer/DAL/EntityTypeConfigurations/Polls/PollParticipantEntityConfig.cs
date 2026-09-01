using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Polls;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Polls
{
    internal class PollParticipantEntityConfig : IEntityTypeConfiguration<PollParticipant>
    {
        public void Configure(EntityTypeBuilder<PollParticipant> builder)
        {
            builder.ToTable("PollParticipants");

            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.HasIndex(x => new { x.PollId, x.ApplicationUserId })
                .IsUnique()
                .HasDatabaseName("IX_PollParticipants_PollId_ApplicationUserId");

            builder.HasOne(x => x.Poll)
                .WithMany()
                .HasForeignKey(x => x.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
