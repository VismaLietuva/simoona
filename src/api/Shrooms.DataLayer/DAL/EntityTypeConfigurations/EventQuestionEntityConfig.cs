using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    public class EventQuestionEntityConfig : IEntityTypeConfiguration<EventQuestion>
    {
        public void Configure(EntityTypeBuilder<EventQuestion> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict rather than Cascade to state the intent that a trigger option is not a
            // disposable row. Note it can never fire in practice: SoftDeleteHandler rewrites every
            // delete into an IsDeleted update, so no DELETE reaches the database.
            builder.HasOne(e => e.ShowIfOption)
                .WithMany()
                .HasForeignKey(e => e.ShowIfOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
