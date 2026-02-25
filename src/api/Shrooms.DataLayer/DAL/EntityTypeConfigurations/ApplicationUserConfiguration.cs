using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Notifications;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Soft delete query filter
            builder.HasQueryFilter(e => !e.IsDeleted);
            
            builder.ToTable("AspNetUsers");

            builder.Property(u => u.BirthDay)
                .IsRequired(false);

            builder.Property(u => u.IsAnonymized)
                .IsRequired();

            // Many-to-many: ApplicationUser <-> Exam
            builder.HasMany(u => u.Exams)
                .WithMany(e => e.ApplicationUsers)
                .UsingEntity(j => j.ToTable("ApplicationUserExams"));

            // Many-to-many: ApplicationUser <-> Skill
            builder.HasMany(u => u.Skills)
                .WithMany(s => s.ApplicationUsers)
                .UsingEntity(j => j.ToTable("ApplicationUserSkills"));

            // Self-referencing: Manager -> ManagedUsers
            builder.HasMany(u => u.ManagedUsers)
                .WithOne(u => u.Manager)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ASP.NET Core Identity relationships - these are already configured by IdentityDbContext
            // Keeping them commented out to avoid conflicts
            /*
            builder.HasMany(u => u.Roles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId);

            builder.HasMany(u => u.Claims)
                .WithOne()
                .HasForeignKey(uc => uc.UserId);

            builder.HasMany(u => u.Logins)
                .WithOne()
                .HasForeignKey(ul => ul.UserId);
            */

            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.Email)
                .HasMaxLength(256);

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("Email");

            // One-to-one: ApplicationUser -> WorkingHours
            builder.HasOne(u => u.WorkingHours)
                .WithOne(w => w.ApplicationUser)
                .HasForeignKey<WorkingHours>("ApplicationUserId");

            builder.Property(u => u.IsManagingDirector)
                .IsRequired();

            // One-to-many: ApplicationUser -> Events
            builder.HasMany(e => e.Events)
                .WithOne()
                .HasForeignKey(e => e.ResponsibleUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-one: ApplicationUser -> Organization
            builder.HasOne(u => u.Organization)
                .WithMany()
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-many: ApplicationUser -> OwnedProjects
            builder.HasMany(u => u.OwnedProjects)
                .WithOne()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-many: ApplicationUser <-> Committee (Members)
            builder.HasMany(u => u.Committees)
                .WithMany(c => c.Members)
                .UsingEntity(j => j.ToTable("CommitteesUsersMembership"));

            // Many-to-many: ApplicationUser <-> Committee (Delegates)
            builder.HasMany(u => u.DelegatingCommittees)
                .WithMany(c => c.Delegates)
                .UsingEntity(j => j.ToTable("CommitteesUsersDelegates"));

            // Many-to-many: ApplicationUser <-> Committee (Leads)
            builder.HasMany(u => u.LeadingCommittees)
                .WithMany(c => c.Leads)
                .UsingEntity(j => j.ToTable("CommitteesUsersLeadership"));

            // One-to-one: ApplicationUser -> NotificationsSettings
            builder.HasOne(u => u.NotificationsSettings)
                .WithOne(s => s.ApplicationUser)
                .HasForeignKey<NotificationsSettings>("ApplicationUserId");
        }
    }
}
