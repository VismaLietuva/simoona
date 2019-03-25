using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.EntityModels.Models;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class ApplicationUserConfiguration : EntityTypeConfiguration<ApplicationUser>
    {
        public ApplicationUserConfiguration()
        {
            Map(entityMappingConfigurationAction: e => e.Requires(discriminator: "IsDeleted").HasValue(value: false))
                .ToTable(tableName: "AspNetUsers");

            Property(propertyExpression: u => u.BirthDay)
                .IsOptional();

            HasMany(navigationPropertyExpression: u => u.Exams)
                .WithMany(navigationPropertyExpression: e => e.ApplicationUsers)
                .Map(configurationAction: t => t.MapLeftKey("ApplicationUserId")
                        .MapRightKey("ExamId")
                        .ToTable(tableName: "ApplicationUserExams"));

            HasMany(navigationPropertyExpression: u => u.Skills)
                .WithMany(navigationPropertyExpression: s => s.ApplicationUsers)
                .Map(configurationAction: t => t.MapLeftKey("ApplicationUserId")
                    .MapRightKey("SkillId")
                    .ToTable(tableName: "ApplicationUserSkills"));

            HasMany(navigationPropertyExpression: u => u.ManagedUsers)
                .WithOptional()
                .HasForeignKey(foreignKeyExpression: u => u.ManagerId);

            HasMany<IdentityUserRole>(navigationPropertyExpression: u => u.Roles)
                .WithRequired()
                .HasForeignKey<string>(foreignKeyExpression: ur => ur.UserId);

            HasMany<IdentityUserClaim>(navigationPropertyExpression: u => u.Claims)
                .WithRequired()
                .HasForeignKey<string>(foreignKeyExpression: uc => uc.UserId);

            HasMany<IdentityUserLogin>(navigationPropertyExpression: u => u.Logins)
                .WithRequired()
                .HasForeignKey<string>(foreignKeyExpression: ul => ul.UserId);

            Property(propertyExpression: u => u.UserName)
                .IsRequired()
                .HasMaxLength(value: 256);

            Property(propertyExpression: u => u.Email)
                .HasMaxLength(value: 256)
                .HasColumnAnnotation(
                    name: IndexAnnotation.AnnotationName,
                    value: new IndexAnnotation(
                        indexAttribute: new IndexAttribute(name: "Email") { IsUnique = true }));

            HasOptional(navigationPropertyExpression: u => u.WorkingHours)
                .WithRequired(navigationPropertyExpression: w => w.ApplicationUser)
                .Map(configurationAction: m => m.MapKey("ApplicationUserId"));

            Property(propertyExpression: u => u.IsManagingDirector)
                .IsRequired();

            HasMany(navigationPropertyExpression: e => e.Events)
                .WithRequired()
                .HasForeignKey(foreignKeyExpression: e => e.ResponsibleUserId)
                .WillCascadeOnDelete(value: false);

            HasRequired(navigationPropertyExpression: u => u.Organization)
                .WithMany()
                .HasForeignKey(foreignKeyExpression: u => u.OrganizationId)
                .WillCascadeOnDelete(value: false);

            HasMany(navigationPropertyExpression: u => u.OwnedProjects)
                .WithRequired()
                .HasForeignKey(foreignKeyExpression: p => p.OwnerId)
                .WillCascadeOnDelete(value: false);

            HasMany(navigationPropertyExpression: u => u.Committees)
                .WithMany(navigationPropertyExpression: c => c.Members)
                .Map(configurationAction: x =>
                {
                    x.ToTable(tableName: "CommitteesUsersMembership");
                });

            HasMany(navigationPropertyExpression: u => u.DelegatingCommittees)
                .WithMany(navigationPropertyExpression: c => c.Delegates)
                .Map(configurationAction: x =>
                {
                    x.ToTable(tableName: "CommitteesUsersDelegates");
                });

            HasMany(navigationPropertyExpression: u => u.LeadingCommittees)
                .WithMany(navigationPropertyExpression: c => c.Leads)
                .Map(configurationAction: x =>
                {
                    x.ToTable(tableName: "CommitteesUsersLeadership");
                });

            HasOptional(navigationPropertyExpression: u => u.NotificationsSettings)
                .WithOptionalPrincipal(navigationPropertyExpression: s => s.ApplicationUser);
        }
    }
}
