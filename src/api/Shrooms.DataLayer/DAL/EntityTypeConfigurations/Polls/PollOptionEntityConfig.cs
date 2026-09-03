using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Polls;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations.Polls
{
    internal class PollOptionEntityConfig : IEntityTypeConfiguration<PollOption>
    {
        public void Configure(EntityTypeBuilder<PollOption> builder)
        {
            builder.ToTable("PollOptions");

            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(x => x.Text).IsRequired().HasMaxLength(PollOption.MaxTextLength);
            builder.Property(x => x.Created).HasColumnType("datetime2");
            builder.Property(x => x.Modified).HasColumnType("datetime2");

            builder.HasIndex(x => x.PollQuestionId).HasDatabaseName("IX_PollOptions_PollQuestionId");
        }
    }
}
