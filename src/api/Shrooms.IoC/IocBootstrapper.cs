using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Razor.Templating.Core;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.DAL;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Impersonate;
using Shrooms.Domain.Services.Jwt;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Projects;
using Shrooms.Domain.Services.SyncTokens;
using Shrooms.Domain.ServiceValidators.Validators.UserAdministration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.IoC.Modules;
using Shrooms.Premium.IoC.Modules;
using System.Reflection;

namespace Shrooms.IoC
{
    public static class IocBootstrapper
    {
        public static IServiceCollection AddShrooms(this IServiceCollection services)
        {
            // Core registrations
            services.AddScoped<IUnitOfWork2, UnitOfWork2>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IAsyncRunner, AsyncRunner>();
            services.AddScoped<ITenantNameContainer, TenantNameContainer>();

            services.AddScoped<IMailingService, MailingService>();
            services.AddScoped<IMailSendingService, SmtpService>();
            services.AddScoped<IPostNotificationService, PostNotificationService>();
            services.AddScoped<ICommentNotificationService, CommentNotificationService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<ISyncTokenService, SyncTokenService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IImpersonateService, ImpersonateService>();
            services.AddScoped<IUserAdministrationValidator, UserAdministrationValidator>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IProjectsService, ProjectsService>();

            // Modules
            services.AddIdentityRegistrations();
            services.AddInfrastructure();
            services.AddWall();
            services.AddKudos();
            services.AddKudosBasket();
            services.AddWebHookCallbacks();
            services.AddRefreshTokens();
            services.AddExternalLinks();
            services.AddRoles();
            services.AddMonitors();
            services.AddSupport();
            services.AddAdministrationUsers();
            services.AddJobs();
            services.AddFilterPresets();
            services.AddBlacklistUsers();
            services.AddCustomEmoji();
            services.AddSeats();
            services.AddEmployees();
            services.AddWidgets();
            services.AddShroomsServices();

            // Premium modules
            services.AddPremiumBackgroundWorkers();
            services.AddPremiumBadges();
            services.AddPremiumBooks();
            services.AddPremiumCommittee();
            services.AddPremiumGroup();
            services.AddPremiumEvents();
            services.AddPremiumKudosShop();
            services.AddPremiumLottery();
            services.AddPremiumOfficeMap();
            services.AddPremiumOrganizationalStructure();
            services.AddPremiumServiceRequest();
            services.AddPremiumServices();
            services.AddPremiumUserEvents();
            services.AddPremiumVacation();
            services.AddPremiumWebHookCallbacks();

            // AutoMapper
            services.AddAutoMapper(
                Assembly.Load("Shrooms.Presentation.ModelMappings"),
                Assembly.Load("Shrooms.Premium"));

            // Must come after every other registration so views can resolve injected services.
            services.AddRazorTemplating();

            return services;
        }
    }
}
