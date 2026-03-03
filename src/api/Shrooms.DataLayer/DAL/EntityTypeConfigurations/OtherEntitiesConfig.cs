using Microsoft.EntityFrameworkCore;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Badges;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Notifications;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class OtherEntitiesConfig
    {
        private readonly ModelBuilder _modelBuilder;

        public OtherEntitiesConfig(ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        public void Add()
        {
            _modelBuilder.Entity<KudosType>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<JobPosition>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<KudosShopItem>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<Floor>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<RoomType>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<Picture>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<Comment>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<Post>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<WorkingHours>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<Exam>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<ServiceRequest>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<ServiceRequestCategory>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<ServiceRequestPriority>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<ServiceRequestStatus>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<ServiceRequestComment>()
                .HasQueryFilter(e => !e.IsDeleted);
            _modelBuilder.Entity<CommitteeSuggestion>()
                .HasQueryFilter(m => !m.IsDeleted);
            _modelBuilder.Entity<CommitteeSuggestion>()
                .Property<string>("UserId").IsRequired();
            _modelBuilder.Entity<Project>()
                .HasQueryFilter(m => !m.IsDeleted);
            _modelBuilder.Entity<NotificationsSettings>()
                .HasQueryFilter(e => !e.IsDeleted);

            // LikesCollection is an owned type stored as a JSON column on Post and Comment
            _modelBuilder.Entity<Post>()
                .OwnsOne(p => p.Likes, b => b.Property(x => x.Serialized).HasColumnName("Likes"));
            _modelBuilder.Entity<Comment>()
                .OwnsOne(c => c.Likes, b => b.Property(x => x.Serialized).HasColumnName("Likes"));

            // ImageCollection is an owned type stored as a JSON column on Post and Comment
            _modelBuilder.Entity<Post>()
                .OwnsOne(p => p.Images, b => b.Property(x => x.Serialized).HasColumnName("Images"));
            _modelBuilder.Entity<Comment>()
                .OwnsOne(c => c.Images, b => b.Property(x => x.Serialized).HasColumnName("Images"));

            // Sources is an owned type on Notification stored as a JSON column
            _modelBuilder.Entity<Notification>()
                .OwnsOne(n => n.Sources, b =>
                {
                    b.Property(x => x.Serialized).HasColumnName("Sources");
                    b.Ignore(x => x.EventId);
                    b.Ignore(x => x.PostId);
                    b.Ignore(x => x.ProjectId);
                    b.Ignore(x => x.WallId);
                });

            _modelBuilder.Entity<Exam>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<QualificationLevel>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<Floor>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<Floor>()
                .HasOne(f => f.Picture)
                .WithMany();

            _modelBuilder.Entity<Picture>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<RoomType>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<WorkingHours>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<QualificationLevel>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<ServiceRequestComment>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<ServiceRequest>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<SyncToken>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            _modelBuilder.Entity<Project>()
                .HasMany(p => p.Attributes)
                .WithMany(s => s.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectSkills",
                    j => j.HasOne<Skill>().WithMany().HasForeignKey("Skill_Id"),
                    j => j.HasOne<Project>().WithMany().HasForeignKey("Project_Id"));

            _modelBuilder.Entity<Project>()
                .HasMany(p => p.Members)
                .WithMany(u => u.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectApplicationUsers",
                    j => j.HasOne<ApplicationUser>().WithMany().HasForeignKey("ApplicationUser_Id"),
                    j => j.HasOne<Project>().WithMany().HasForeignKey("Project_Id"));

            _modelBuilder.Entity<Exam>()
                .HasMany(e => e.Certificates)
                .WithMany(c => c.Exams)
                .UsingEntity<Dictionary<string, object>>(
                    "ExamCertificates",
                    j => j.HasOne<Certificate>().WithMany().HasForeignKey("Certificate_Id"),
                    j => j.HasOne<Exam>().WithMany().HasForeignKey("Exam_Id"));

            _modelBuilder.Entity<ServiceRequestCategory>()
                .HasMany(s => s.Assignees)
                .WithMany(u => u.ServiceRequestCategoriesAssigned)
                .UsingEntity<Dictionary<string, object>>(
                    "ServiceRequestCategoryApplicationUsers",
                    j => j.HasOne<ApplicationUser>().WithMany().HasForeignKey("ApplicationUser_Id"),
                    j => j.HasOne<ServiceRequestCategory>().WithMany().HasForeignKey("ServiceRequestCategory_Id"));

            // These entities inherit from BaseModel (which has IsDeleted) but their DB tables
            // do not have the IsDeleted column, so the property must be ignored.
            _modelBuilder.Entity<QualificationLevel>().Ignore(e => e.IsDeleted);
            _modelBuilder.Entity<Skill>().Ignore(e => e.IsDeleted);
            _modelBuilder.Entity<BadgeLog>().Ignore(e => e.IsDeleted);
            _modelBuilder.Entity<BadgeType>().Ignore(e => e.IsDeleted);
            _modelBuilder.Entity<BadgeCategory>().Ignore(e => e.IsDeleted);
            _modelBuilder.Entity<Lottery>().Ignore(e => e.IsDeleted);
            _modelBuilder.Entity<LotteryParticipant>().Ignore(e => e.IsDeleted);

            // DbSet property names now match the DB table names for these entities.
        }
    }
}
