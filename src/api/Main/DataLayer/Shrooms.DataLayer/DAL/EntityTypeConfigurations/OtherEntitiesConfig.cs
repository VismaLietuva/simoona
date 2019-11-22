using System.Data.Entity;
using DataLayer.Models;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.EntityModels.Models.Notifications;

namespace Shrooms.DataLayer.DAL.EntityTypeConfigurations
{
    internal class OtherEntitiesConfig
    {
        private readonly DbModelBuilder _modelBuilder;

        public OtherEntitiesConfig(DbModelBuilder modelBuilder)
        {
            this._modelBuilder = modelBuilder;
        }

        public void Add()
        {
            _modelBuilder.Entity<KudosType>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<JobPosition>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<KudosShopItem>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<Floor>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<RoomType>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<Picture>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<Comment>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<Post>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<WorkingHours>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<Exam>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<ServiceRequest>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<ServiceRequestCategory>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<ServiceRequestPriority>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<ServiceRequestStatus>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<ServiceRequestComment>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<CommitteeSuggestion>()
                .Map(m => m.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<Project>()
                .Map(m => m.Requires("IsDeleted").HasValue(false));
            _modelBuilder.Entity<NotificationsSettings>()
                .Map(e => e.Requires("IsDeleted").HasValue(false));
            _modelBuilder.ComplexType<LikesCollection>()
                .Property(x => x.Serialized)
                .HasColumnName("Likes");
            _modelBuilder.ComplexType<ImagesCollection>()
                .Property(x => x.Serialized)
                .HasColumnName("Images");
            _modelBuilder.ComplexType<OfficesCollection>()
                .Property(x => x.Serialized)
                .HasColumnName("Offices");
            _modelBuilder.ComplexType<Sources>()
                .Property(x => x.Serialized)
                .HasColumnName("Sources");
            _modelBuilder.ComplexType<Sources>()
                .Ignore(x => x.EventId)
                .Ignore(x => x.PostId)
                .Ignore(x => x.ProjectId)
                .Ignore(x => x.WallId);

            _modelBuilder.Entity<Exam>()
                .HasRequired(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .WillCascadeOnDelete(false);

            _modelBuilder.Entity<QualificationLevel>()
                .HasRequired(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .WillCascadeOnDelete(false);

            _modelBuilder.Entity<Floor>()
                .HasRequired(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .WillCascadeOnDelete(false);

            _modelBuilder.Entity<Picture>()
                .HasRequired(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .WillCascadeOnDelete(false);

            _modelBuilder.Entity<RoomType>()
                .HasRequired(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .WillCascadeOnDelete(false);

            _modelBuilder.Entity<WorkingHours>()
                .HasRequired(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .WillCascadeOnDelete(false);

            _modelBuilder.Entity<QualificationLevel>()
                .HasRequired(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .WillCascadeOnDelete(false);

            _modelBuilder.Entity<ServiceRequestComment>()
                .HasRequired(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .WillCascadeOnDelete(false);

            _modelBuilder.Entity<ServiceRequest>()
                .HasRequired(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .WillCascadeOnDelete(false);

            _modelBuilder.Entity<SyncToken>()
                .HasRequired(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .WillCascadeOnDelete(false);

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
