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

            // Both validators treat a question's Order as its identity in the tree: uniqueness is
            // only checked per request, so two hosts saving the same event concurrently could
            // otherwise persist a tie, and every read projection sorts on Order with no secondary
            // key. Filtered on IsDeleted so reusing the order of a soft-deleted question still works.
            builder.HasIndex(e => new { e.EventId, e.Order })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasOne(e => e.Event)
                .WithMany(e => e.EventQuestions)
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
