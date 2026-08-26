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
                .WithMany(e => e.EventQuestions)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict, not Cascade: deleting a trigger option must not silently rewrite the
            // question tree by nulling conditions. The structure validator rejects that state.
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
