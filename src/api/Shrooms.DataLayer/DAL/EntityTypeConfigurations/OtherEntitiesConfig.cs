using Microsoft.EntityFrameworkCore;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
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
            _modelBuilder.Entity<Project>()
                .HasQueryFilter(m => !m.IsDeleted);
            _modelBuilder.Entity<NotificationsSettings>()
                .HasQueryFilter(e => !e.IsDeleted);

            _modelBuilder.Entity<LikesCollection>()
                .Property(x => x.Serialized)
                .HasColumnName("Likes");
            _modelBuilder.Entity<ImageCollection>()
                .Property(x => x.Serialized)
                .HasColumnName("Images");
            _modelBuilder.Entity<Sources>()
                .Property(x => x.Serialized)
                .HasColumnName("Sources");
            _modelBuilder.Entity<Sources>()
                .Ignore(x => x.EventId)
                .Ignore(x => x.PostId)
                .Ignore(x => x.ProjectId)
                .Ignore(x => x.WallId);

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
                .HasMany(p => p.Attributes)
                .WithMany(s => s.Projects);

            _modelBuilder.Entity<Project>()
                .HasMany(p => p.Members)
                .WithMany(u => u.Projects);

            _modelBuilder.Entity<Project>()
                .ToTable("Projects");
        }
    }
}
