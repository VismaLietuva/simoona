using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Polls;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Polls
{
    internal class PollAnswerEntityConfig : IEntityTypeConfiguration<PollAnswer>
    {
        public void Configure(EntityTypeBuilder<PollAnswer> builder)
        {
            builder.ToTable("PollAnswers");

            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.HasIndex(x => x.PollId).HasDatabaseName("IX_PollAnswers_PollId");
            builder.HasIndex(x => x.PollOptionId).HasDatabaseName("IX_PollAnswers_PollOptionId");

            builder.HasOne(x => x.Poll)
                .WithMany()
                .HasForeignKey(x => x.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Question)
                .WithMany()
                .HasForeignKey(x => x.PollQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Option)
                .WithMany()
                .HasForeignKey(x => x.PollOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
