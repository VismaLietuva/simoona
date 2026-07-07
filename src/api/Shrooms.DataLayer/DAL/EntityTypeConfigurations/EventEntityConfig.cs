using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class EventEntityConfig : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);

            builder.Property(e => e.Name)
                .IsRequired();

            builder.Property(e => e.Place)
                .IsRequired();

            builder.Property(e => e.MaxParticipants)
                .IsRequired();

            builder.Property(e => e.StartDate)
                .IsRequired();

            builder.HasIndex(e => e.StartDate)
                .HasDatabaseName("ix_start_date");

            builder.Property(e => e.EndDate)
                .IsRequired();

            builder.HasIndex(e => e.EndDate)
                .HasDatabaseName("ix_end_date");

            builder.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.EventOptions)
                .WithOne(e => e.Event)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Reminders)
                .WithOne(e => e.Event)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
