using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class ApplicationUserConfiguration : EntityTypeConfiguration<ApplicationUser>
    {
        public ApplicationUserConfiguration()
        {
            Map(e => e.Requires("IsDeleted").HasValue(value: false))
                .ToTable("AspNetUsers");

            Property(u => u.BirthDay)
                .IsOptional();

            HasMany(u => u.Exams)
                .WithMany(e => e.ApplicationUsers)
                .Map(t => t.MapLeftKey("ApplicationUserId")
                        .MapRightKey("ExamId")
                        .ToTable("ApplicationUserExams"));

            HasMany(u => u.Skills)
                .WithMany(s => s.ApplicationUsers)
                .Map(t => t.MapLeftKey("ApplicationUserId")
                    .MapRightKey("SkillId")
                    .ToTable("ApplicationUserSkills"));

            HasMany(u => u.ManagedUsers)
                .WithOptional()
                .HasForeignKey(u => u.ManagerId);

            HasMany(u => u.Roles)
                .WithRequired()
                .HasForeignKey(ur => ur.UserId);

            HasMany(u => u.Claims)
                .WithRequired()
                .HasForeignKey(uc => uc.UserId);

            HasMany(u => u.Logins)
                .WithRequired()
                .HasForeignKey(ul => ul.UserId);

            Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(value: 256);

            Property(u => u.Email)
                .HasMaxLength(value: 256)
                .HasColumnAnnotation(
                    name: IndexAnnotation.AnnotationName,
                    value: new IndexAnnotation(
                        indexAttribute: new IndexAttribute("Email") { IsUnique = true }));

            HasOptional(u => u.WorkingHours)
                .WithRequired(w => w.ApplicationUser)
                .Map(m => m.MapKey("ApplicationUserId"));

            Property(u => u.IsManagingDirector)
                .IsRequired();

            HasMany(e => e.Events)
                .WithRequired()
                .HasForeignKey(e => e.ResponsibleUserId)
                .WillCascadeOnDelete(value: false);

            HasRequired(u => u.Organization)
                .WithMany()
                .HasForeignKey(u => u.OrganizationId)
                .WillCascadeOnDelete(value: false);

            HasMany(u => u.OwnedProjects)
                .WithRequired()
                .HasForeignKey(p => p.OwnerId)
                .WillCascadeOnDelete(value: false);

            HasMany(u => u.Committees)
                .WithMany(c => c.Members)
                .Map(x =>
                {
                    x.ToTable("CommitteesUsersMembership");
                });

            HasMany(u => u.DelegatingCommittees)
                .WithMany(c => c.Delegates)
                .Map(x =>
                {
                    x.ToTable("CommitteesUsersDelegates");
                });

            HasMany(u => u.LeadingCommittees)
                .WithMany(c => c.Leads)
                .Map(x =>
                {
                    x.ToTable("CommitteesUsersLeadership");
                });

            HasOptional(u => u.NotificationsSettings)
                .WithOptionalPrincipal(s => s.ApplicationUser);
        }
    }
}
