using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.DAL.EntityTypeConfigurations;
using Shrooms.DataLayer.DAL.EntityTypeConfigurations.Badges;
using Shrooms.DataLayer.DAL.EntityTypeConfigurations.Seats;
using Shrooms.DataLayer.DAL.EntityTypeConfigurations.Vacations;
using Shrooms.DataLayer.DAL.EntityTypeConfigurations.VideoLibrary;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Badges;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.DataLayer.EntityModels.Models.Emoji;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.DataLayer.EntityModels.Models.Monitors;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Notifications;
using Shrooms.DataLayer.EntityModels.Models.Group;
using Shrooms.DataLayer.EntityModels.Models.Seats;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;
using GroupEntity = Shrooms.DataLayer.EntityModels.Models.Group.Group;

namespace Shrooms.DataLayer.DAL
{
    public class ShroomsDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IDbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShroomsDbContext()
        {
        }

        public ShroomsDbContext(DbContextOptions<ShroomsDbContext> options, IHttpContextAccessor httpContextAccessor = null)
            : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
            _httpContextAccessor = httpContextAccessor;
        }

        // Tenant key the DbContext was resolved for. Assigned by the DI factory so
        // background workers (AsyncRunner) can flow the tenant via _uow.ConnectionName.
        public string ConnectionName { get; set; }

        public virtual DbSet<Office> Offices { get; set; }

        public virtual DbSet<Floor> Floors { get; set; }

        public virtual DbSet<Room> Rooms { get; set; }

        public virtual DbSet<RoomType> RoomTypes { get; set; }

        public virtual DbSet<Organization> Organizations { get; set; }

        public virtual DbSet<Page> Pages { get; set; }

        public virtual DbSet<Permission> Permissions { get; set; }

        public virtual DbSet<QualificationLevel> QualificationLevels { get; set; }

        public virtual DbSet<AbstractClassifier> AbstractClassifiers { get; set; }

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

        public virtual DbSet<EventReminder> EventReminders { get; set; }

        public virtual DbSet<EventParticipant> EventParticipants { get; set; }

        public virtual DbSet<EventOption> EventOptions { get; set; }

        public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }

        public virtual DbSet<ServiceRequestCategory> ServiceRequestCategories { get; set; }

        public virtual DbSet<ServiceRequestPriority> ServiceRequestPriorities { get; set; }

        public virtual DbSet<ServiceRequestStatus> ServiceRequestStatus { get; set; }

        public virtual DbSet<ServiceRequestComment> ServiceRequestComments { get; set; }

        public virtual DbSet<Committee> Committees { get; set; }

        public virtual DbSet<GroupType> GroupTypes { get; set; }

        public virtual DbSet<GroupEntity> Groups { get; set; }

        public virtual DbSet<GroupMember> GroupMembers { get; set; }

        public virtual DbSet<GroupReference> GroupReferences { get; set; }

        public virtual DbSet<Book> Books { get; set; }

        public virtual DbSet<BookLog> BookLogs { get; set; }

        public virtual DbSet<BookOffice> BookOffices { get; set; }

        public virtual DbSet<SyncToken> SyncTokens { get; set; }

        public virtual DbSet<Module> Modules { get; set; }

        public virtual DbSet<KudosBasket> KudosBaskets { get; set; }

        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

        public virtual DbSet<Wall> Walls { get; set; }

        public virtual DbSet<WallMember> WallMembers { get; set; }

        public virtual DbSet<WallModerator> WallModerators { get; set; }

        public virtual DbSet<ExternalLink> ExternalLinks { get; set; }

        public virtual DbSet<EntityModels.Models.Monitors.Monitor> Monitors { get; set; }

        public virtual DbSet<JobPosition> JobPositions { get; set; }

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

        public virtual DbSet<BlacklistUser> BlacklistUsers { get; set; }

        public virtual DbSet<Banner> Banners { get; set; }

        public virtual DbSet<CommitteeSuggestion> CommitteeSuggestions { get; set; }

        public virtual DbSet<NotificationUser> NotificationUsers { get; set; }

        public virtual DbSet<PostWatcher> PostWatchers { get; set; }

        public virtual DbSet<CustomEmoji> CustomEmojis { get; set; }

        public virtual DbSet<VacationRequest> VacationRequests { get; set; }

        public virtual DbSet<VacationRequestEvent> VacationRequestEvents { get; set; }

        public virtual DbSet<VacationOrder> VacationOrders { get; set; }

        public virtual DbSet<VacationOrderItem> VacationOrderItems { get; set; }

        public virtual DbSet<Holiday> Holidays { get; set; }

        public virtual DbSet<Seat> Seats { get; set; }

        public virtual DbSet<SeatReservation> SeatReservations { get; set; }

        public virtual DbSet<SeatRelease> SeatReleases { get; set; }

        public virtual DbSet<VideoType> VideoTypes { get; set; }

        public virtual DbSet<VideoLibraryItem> VideoLibraryItems { get; set; }

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

        public new int SaveChanges(bool useMetaTracking = true)
        {
            if (useMetaTracking)
            {
                UpdateEntityMetadata(ChangeTracker.Entries());
            }

            SoftDeleteHandler.Execute(ChangeTracker.Entries(), this);

            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply all entity type configurations
            modelBuilder.ApplyConfiguration(new KudosBasketEntityConfig());
            modelBuilder.ApplyConfiguration(new KudosLogEntityConfig());
            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
            // Identity configurations are handled by ASP.NET Core Identity
            // modelBuilder.ApplyConfiguration(new IdentityUserRoleEntityConfig());
            // modelBuilder.ApplyConfiguration(new IdentityUserLoginEntityConfig());
            // modelBuilder.ApplyConfiguration(new IdentityUserClaimEntityConfig());
            modelBuilder.ApplyConfiguration(new ApplicationRoleConfiguration());
            modelBuilder.ApplyConfiguration(new AbstractClassifierConfiguration());
            modelBuilder.ApplyConfiguration(new RoomEntityConfig());
            modelBuilder.ApplyConfiguration(new PageEntityConfig());
            modelBuilder.ApplyConfiguration(new PermissionEntityConfig());
            modelBuilder.ApplyConfiguration(new ModuleEntityConfig());
            modelBuilder.ApplyConfiguration(new EventEntityConfig());
            modelBuilder.ApplyConfiguration(new EventTypeEntityConfig());
            modelBuilder.ApplyConfiguration(new EventParticipantEntityConfig());
            modelBuilder.ApplyConfiguration(new EventOptionEntityConfig());
            modelBuilder.ApplyConfiguration(new CommitteeEntityConfig());
            modelBuilder.ApplyConfiguration(new GroupTypeEntityConfig());
            modelBuilder.ApplyConfiguration(new GroupEntityConfig());
            modelBuilder.ApplyConfiguration(new GroupMemberEntityConfig());
            modelBuilder.ApplyConfiguration(new GroupReferenceEntityConfig());
            modelBuilder.ApplyConfiguration(new BookOfficeEntityConfig());
            modelBuilder.ApplyConfiguration(new BookLogEntityConfig());
            modelBuilder.ApplyConfiguration(new BookEntityConfig());
            modelBuilder.ApplyConfiguration(new OfficeEntityConfig());
            modelBuilder.ApplyConfiguration(new OrganizationEntityConfig());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new WallConfiguration());
            modelBuilder.ApplyConfiguration(new WallMembersConfiguration());
            modelBuilder.ApplyConfiguration(new WallModeratorsConfiguration());
            modelBuilder.ApplyConfiguration(new PostEntityConfig());
            modelBuilder.ApplyConfiguration(new ExternalLinkConfig());
            modelBuilder.ApplyConfiguration(new MonitorConfig());
            modelBuilder.ApplyConfiguration(new NotificationConfig());
            modelBuilder.ApplyConfiguration(new NotifiationUserConfig());
            modelBuilder.ApplyConfiguration(new PostWatcherConfig());
            modelBuilder.ApplyConfiguration(new VacationEntityConfig());
            modelBuilder.ApplyConfiguration(new FilterPresetEntityConfig());
            modelBuilder.ApplyConfiguration(new BlacklistUserEntityConfig());
            modelBuilder.ApplyConfiguration(new EventReminderEntityConfig());
            modelBuilder.ApplyConfiguration(new BannerEntityConfiguration());
            modelBuilder.ApplyConfiguration(new BadgeCategoryKudosTypeEntityConfiguration());
            modelBuilder.ApplyConfiguration(new BadgeCategoryEntityConfiguration());
            modelBuilder.ApplyConfiguration(new BadgeLogEntityConfiguration());
            modelBuilder.ApplyConfiguration(new BadgeTypeEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CustomEmojiConfig());
            modelBuilder.ApplyConfiguration(new VacationRequestEntityConfig());
            modelBuilder.ApplyConfiguration(new VacationRequestEventEntityConfig());
            modelBuilder.ApplyConfiguration(new VacationOrderEntityConfig());
            modelBuilder.ApplyConfiguration(new VacationOrderItemEntityConfig());
            modelBuilder.ApplyConfiguration(new HolidayEntityConfig());
            modelBuilder.ApplyConfiguration(new SeatEntityConfig());
            modelBuilder.ApplyConfiguration(new SeatReservationEntityConfig());
            modelBuilder.ApplyConfiguration(new SeatReleaseEntityConfig());
            modelBuilder.ApplyConfiguration(new VideoTypeEntityConfig());
            modelBuilder.ApplyConfiguration(new VideoLibraryItemEntityConfig());

            new OtherEntitiesConfig(modelBuilder).Add();

            // EntityModels uses Nullable: enable, so non-nullable string properties (string without ?)
            // are treated by EF Core as required by convention. But the legacy database allows NULL in
            // most string columns. This pass relaxes string properties that were made required ONLY by
            // convention (not via explicit fluent config or [Required] annotation).
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(string) && !p.IsKey() && !p.IsNullable
                                && p.PropertyInfo != null)) // skip shadow properties (e.g. TPH discriminator)
                {
                    var conventionProperty = (Microsoft.EntityFrameworkCore.Metadata.IConventionProperty)property;
                    var source = conventionProperty.GetIsNullableConfigurationSource();
                    // Only relax properties whose nullability was set by convention (not explicit config)
                    if (source == null || source == Microsoft.EntityFrameworkCore.Metadata.ConfigurationSource.Convention)
                    {
                        conventionProperty.SetIsNullable(true);
                    }
                }
            }

            // Brownfield databases key AspNetUsers/AspNetRoles at nvarchar(128)
            // (see AddMissingIdentityTables, which derives its FK widths from the
            // live column), while EF Core's Identity default is 450. SQL Server
            // refuses a foreign key whose length differs from the key it
            // references, so the length is declared once here and inherited by
            // every entity: relying on each config to remember it is how
            // VacationOrders and Seats both shipped an FK no brownfield database
            // could create.
            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.Id)
                .HasMaxLength(DataLayerConstants.IdentityKeyLength);

            modelBuilder.Entity<ApplicationRole>()
                .Property(role => role.Id)
                .HasMaxLength(DataLayerConstants.IdentityKeyLength);

            // The Identity join tables are created by AddMissingIdentityTables,
            // not by EF: it sizes AspNetUserLogins from the live AspNetUsers.Id and
            // hard-codes 128 for AspNetUserTokens. These four key columns are
            // therefore 128 in every database, fresh ones included.
            modelBuilder.Entity<IdentityUserLogin<string>>(login =>
            {
                login.Property(x => x.LoginProvider).HasMaxLength(DataLayerConstants.IdentityKeyLength);
                login.Property(x => x.ProviderKey).HasMaxLength(DataLayerConstants.IdentityKeyLength);
            });

            modelBuilder.Entity<IdentityUserToken<string>>(token =>
            {
                token.Property(x => x.LoginProvider).HasMaxLength(DataLayerConstants.IdentityKeyLength);
                token.Property(x => x.Name).HasMaxLength(DataLayerConstants.IdentityKeyLength);
            });

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var identityForeignKeys = entityType.GetForeignKeys()
                    .Where(foreignKey =>
                        foreignKey.PrincipalEntityType.ClrType == typeof(ApplicationUser) ||
                        foreignKey.PrincipalEntityType.ClrType == typeof(ApplicationRole));

                foreach (var foreignKey in identityForeignKeys)
                {
                    foreach (var property in foreignKey.Properties)
                    {
                        property.SetMaxLength(DataLayerConstants.IdentityKeyLength);
                    }
                }
            }
        }

        // EF Core's standard SaveChangesAsync(CancellationToken) is called by Identity's UserManager
        // and other framework code that bypasses our custom SaveChangesAsync(bool) overloads.
        // Overriding it here ensures UpdateEntityMetadata always runs, preventing DateTime.MinValue
        // from being written to brownfield datetime columns (which only accept dates >= 1753-01-01).
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            UpdateEntityMetadata(ChangeTracker.Entries());
            await SoftDeleteHandler.ExecuteAsync(ChangeTracker.Entries(), this);
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateEntityMetadata(IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entries, string userId = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            var now = DateTime.UtcNow;
            var items = entries
                .Where(p => p.Entity is ITrackable)
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
                    // Guard against DateTime.MinValue from brownfield NULL columns —
                    // 0001-01-01 is out of range for SQL Server's datetime type.
                    if (item.Entity.Created == default)
                    {
                        item.Entity.Created = now;
                    }
                }
            }
        }
    }
}
