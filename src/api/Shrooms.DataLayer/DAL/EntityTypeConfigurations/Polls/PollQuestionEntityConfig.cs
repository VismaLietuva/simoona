using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Polls;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Polls
{
    internal class PollQuestionEntityConfig : IEntityTypeConfiguration<PollQuestion>
    {
        public void Configure(EntityTypeBuilder<PollQuestion> builder)
        {
            builder.ToTable("PollQuestions");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(x => x.Text).IsRequired().HasMaxLength(PollQuestion.MaxTextLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => x.PollId).HasDatabaseName("IX_PollQuestions_PollId");

            builder.HasMany(x => x.Options)
                .WithOne(x => x.Question)
                .HasForeignKey(x => x.PollQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
