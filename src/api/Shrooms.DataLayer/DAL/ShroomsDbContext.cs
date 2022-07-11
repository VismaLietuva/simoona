using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.DAL.EntityTypeConfigurations;
using Shrooms.DataLayer.EntityModels.Attributes;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Badges;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.DataLayer.EntityModels.Models.Monitors;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Notifications;

namespace Shrooms.DataLayer.DAL
{
    [DbConfigurationType(typeof(ShroomsContextConfiguration))]
    public class ShroomsDbContext : DbContext, IDbContext
    {
        public ShroomsDbContext()
        {
        }

        public ShroomsDbContext(string connectionStringName)
            : base(connectionStringName)
        {
            ConnectionName = connectionStringName;
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Database.SetInitializer<ShroomsDbContext>(null);
        }

        public virtual DbSet<ApplicationUser> Users { get; set; }

        public virtual DbSet<ApplicationRole> Roles { get; set; }

        public virtual DbSet<Office> Offices { get; set; }

        public virtual DbSet<Floor> Floors { get; set; }

        public virtual DbSet<Room> Rooms { get; set; }

        public virtual DbSet<RoomType> RoomTypes { get; set; }

        public virtual DbSet<Organization> Organizations { get; set; }

        public virtual DbSet<Page> Pages { get; set; }

        public virtual DbSet<Permission> Permissions { get; set; }

        public virtual DbSet<QualificationLevel> QualificationLevels { get; set; }

        public virtual DbSet<AbstractClassifier> Classificators { get; set; }

        public virtual DbSet<Picture> Pictures { get; set; }

        public virtual DbSet<Post> Posts { get; set; }

        public virtual DbSet<Comment> Comments { get; set; }

        public virtual DbSet<Certificate> Certificates { get; set; }

        public virtual DbSet<Skill> Skills { get; set; }

        public virtual DbSet<Exam> Exams { get; set; }

        public virtual DbSet<KudosLog> KudosLogs { get; set; }

        public virtual DbSet<KudosType> KudosTypes { get; set; }

        public virtual DbSet<Event> Events { get; set; }

        public virtual DbSet<EventType> EventTypes { get; set; }

        public virtual DbSet<EventParticipant> EventsParticipants { get; set; }

        public virtual DbSet<EventOption> EventOptions { get; set; }

        public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }

        public virtual DbSet<ServiceRequestCategory> ServiceRequestCategories { get; set; }

        public virtual DbSet<ServiceRequestPriority> ServiceRequestPriorities { get; set; }

        public virtual DbSet<ServiceRequestStatus> ServiceRequestStatuses { get; set; }

        public virtual DbSet<ServiceRequestComment> ServiceRequestComments { get; set; }

        public virtual DbSet<Committee> Committees { get; set; }

        public virtual DbSet<Book> Books { get; set; }

        public virtual DbSet<BookLog> BookLogs { get; set; }

        public virtual DbSet<BookOffice> BookOffice { get; set; }

        public virtual DbSet<SyncToken> SyncTokens { get; set; }

        public virtual DbSet<Module> Modules { get; set; }

        public virtual DbSet<KudosBasket> KudosBaskets { get; set; }

        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

        public virtual DbSet<Wall> Walls { get; set; }

        public virtual DbSet<WallMember> WallMembers { get; set; }

        public virtual DbSet<WallModerator> WallModerators { get; set; }

        public virtual DbSet<ExternalLink> ExternalLinks { get; set; }

        public virtual DbSet<Monitor> Monitors { get; set; }

        public virtual DbSet<JobPosition> JobPosition { get; set; }

        public virtual DbSet<Project> Projects { get; set; }

        public virtual DbSet<KudosShopItem> KudosShopItems { get; set; }

        public virtual DbSet<Notification> Notifications { get; set; }

        public virtual DbSet<BadgeCategory> BadgeCategories { get; set; }

        public virtual DbSet<BadgeType> BadgeTypes { get; set; }

        public virtual DbSet<BadgeCategoryKudosType> BadgeCategoryKudosType { get; set; }

        public virtual DbSet<BadgeLog> BadgeLogs { get; set; }

        public virtual DbSet<Lottery> Lotteries { get; set; }

        public virtual DbSet<LotteryParticipant> LotteryParticipants { get; set; }

        public virtual DbSet<VacationPage> VacationPages { get; set; }

        public virtual DbSet<FilterPreset> FilterPresets { get; set; }

        public virtual DbSet<BlacklistState> BlacklistStates { get; set; }

        public string ConnectionName { get; }

        public int SaveChanges(string userId)
        {
            UpdateEntityMetadata(ChangeTracker.Entries(), userId);
            SoftDeleteHandler.Execute(ChangeTracker.Entries(), this);

            return base.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(string userId)
        {
            UpdateEntityMetadata(ChangeTracker.Entries(), userId);
            await SoftDeleteHandler.ExecuteAsync(ChangeTracker.Entries(), this);
            return await base.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync(bool useMetaTracking = true)
        {
            if (useMetaTracking)
            {
                UpdateEntityMetadata(ChangeTracker.Entries());
            }

            await SoftDeleteHandler.ExecuteAsync(ChangeTracker.Entries(), this);
            return await base.SaveChangesAsync();
        }

        public int SaveChanges(bool useMetaTracking = true)
        {
            if (useMetaTracking)
            {
                UpdateEntityMetadata(ChangeTracker.Entries());
            }

            SoftDeleteHandler.Execute(ChangeTracker.Entries(), this);

            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new KudosBasketEntityConfig());
            modelBuilder.Configurations.Add(new KudosLogEntityConfig());
            modelBuilder.Configurations.Add(new ApplicationUserConfiguration());
            modelBuilder.Configurations.Add(new IdentityUserRoleEntityConfig());
            modelBuilder.Configurations.Add(new IdentityUserLoginEntityConfig());
            modelBuilder.Configurations.Add(new IdentityUserClaimEntityConfig());
            modelBuilder.Configurations.Add(new ApplicationRoleConfiguration());
            modelBuilder.Configurations.Add(new AbstractClassifierConfiguration());
            modelBuilder.Configurations.Add(new RoomEntityConfig());
            modelBuilder.Configurations.Add(new PageEntityConfig());
            modelBuilder.Configurations.Add(new PermissionEntityConfig());
            modelBuilder.Configurations.Add(new EventEntityConfig());
            modelBuilder.Configurations.Add(new EventTypeEntityConfig());
            modelBuilder.Configurations.Add(new EventParticipantEntityConfig());
            modelBuilder.Configurations.Add(new EventOptionEntityConfig());
            modelBuilder.Configurations.Add(new CommitteeEntityConfig());
            modelBuilder.Configurations.Add(new BookOfficeEntityConfig());
            modelBuilder.Configurations.Add(new BookLogEntityConfig());
            modelBuilder.Configurations.Add(new BookEntityConfig());
            modelBuilder.Configurations.Add(new OfficeEntityConfig());
            modelBuilder.Configurations.Add(new OrganizationEntityConfig());
            modelBuilder.Configurations.Add(new RefreshTokenConfiguration());
            modelBuilder.Configurations.Add(new WallConfiguration());
            modelBuilder.Configurations.Add(new WallMembersConfiguration());
            modelBuilder.Configurations.Add(new WallModeratorsConfiguration());
            modelBuilder.Configurations.Add(new PostEntityConfig());
            modelBuilder.Configurations.Add(new ExternalLinkConfig());
            modelBuilder.Configurations.Add(new MonitorConfig());
            modelBuilder.Configurations.Add(new NotificationConfig());
            modelBuilder.Configurations.Add(new NotifiationUserConfig());
            modelBuilder.Configurations.Add(new PostWatcherConfig());
            modelBuilder.Configurations.Add(new VacationEntityConfig());
            modelBuilder.Configurations.Add(new FilterPresetEntityConfig());
            modelBuilder.Configurations.Add(new BlacklistStateEntityConfig());

            var convention = new AttributeToColumnAnnotationConvention<SqlDefaultValueAttribute, string>("SqlDefaultValue", (p, attributes) => attributes.Single().DefaultValue);
            modelBuilder.Conventions.Add(convention);

            new OtherEntitiesConfig(modelBuilder).Add();
        }

        private static void UpdateEntityMetadata(IEnumerable<DbEntityEntry> entries, string userId = "")
        {
            if (string.IsNullOrEmpty(userId) && HttpContext.Current != null && HttpContext.Current.User != null)
            {
                userId = HttpContext.Current.User.Identity.GetUserId();
            }

            var now = DateTime.UtcNow;
            var items = entries
                .Where(p => p.Entity is ITrackable && p.Entity is ISoftDelete)
                .Select(x => new
                {
                    x.State,
                    Entity = x.Entity as ITrackable
                });

            foreach (var item in items)
            {
                if (item.State == EntityState.Added)
                {
                    item.Entity.Created = now;
                    item.Entity.Modified = now;
                    item.Entity.CreatedBy = userId;
                }
                else if (item.State == EntityState.Deleted || item.State == EntityState.Modified)
                {
                    item.Entity.Modified = now;
                    item.Entity.ModifiedBy = userId;
                }
            }
        }
    }
}