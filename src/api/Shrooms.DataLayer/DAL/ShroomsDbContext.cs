using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.DAL.EntityTypeConfigurations;
using Shrooms.DataLayer.DAL.EntityTypeConfigurations.Badges;
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
    public class ShroomsDbContext : DbContext, IDbContext
    {
        public ShroomsDbContext()
        {
        }

        public ShroomsDbContext(DbContextOptions<ShroomsDbContext> options)
            : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
        }

        // For backward compatibility - connection string based initialization
        public string ConnectionName { get; private set; }

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

        public virtual DbSet<EventReminder> EventReminders { get; set; }

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

        public virtual DbSet<EntityModels.Models.Monitors.Monitor> Monitors { get; set; }

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

        public virtual DbSet<BlacklistUser> BlacklistUsers { get; set; }

        public virtual DbSet<Banner> Banners { get; set; }

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
            modelBuilder.ApplyConfiguration(new EventEntityConfig());
            modelBuilder.ApplyConfiguration(new EventTypeEntityConfig());
            modelBuilder.ApplyConfiguration(new EventParticipantEntityConfig());
            modelBuilder.ApplyConfiguration(new EventOptionEntityConfig());
            modelBuilder.ApplyConfiguration(new CommitteeEntityConfig());
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

            // TODO: SqlDefaultValue attribute convention needs to be reimplemented for EF Core
            // var convention = new AttributeToColumnAnnotationConvention<SqlDefaultValueAttribute, string>("SqlDefaultValue", (p, attributes) => attributes.Single().DefaultValue);
            // modelBuilder.Conventions.Add(convention);

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
        }

        // TODO: HttpContext.Current is not available in ASP.NET Core
        // This method needs to be updated to receive IHttpContextAccessor through dependency injection
        // For now, userId must be explicitly passed
        private static void UpdateEntityMetadata(IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entries, string userId = "")
        {
            // TODO: In ASP.NET Core, inject IHttpContextAccessor instead of using HttpContext.Current
            // Example:
            // if (string.IsNullOrEmpty(userId) && _httpContextAccessor.HttpContext?.User != null)
            // {
            //     userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // }

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
