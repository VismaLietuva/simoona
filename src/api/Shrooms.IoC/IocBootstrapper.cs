using Autofac;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using AutoMapper;
using Hangfire;
using Microsoft.Owin.Security.DataProtection;
using Newtonsoft.Json;
using Owin;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.DAL;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Impersonate;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Projects;
using Shrooms.Domain.Services.SyncTokens;
using Shrooms.Domain.ServiceValidators.Validators.UserAdministration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.IoC.Modules;
using Shrooms.Premium.IoC.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace Shrooms.IoC
{
    public static class IocBootstrapper
    {
        public static IContainer Bootstrap(IAppBuilder app, Func<string> getConnectionStringName, HttpConfiguration config)
        {
            var builder = new ContainerBuilder();
            var shroomsApi = Assembly.Load("Shrooms.Presentation.Api");
            var common = Assembly.Load("Shrooms.Presentation.Common");
            var premiumApi = Assembly.Load("Shrooms.Premium");
            var dataLayer = Assembly.Load("Shrooms.DataLayer");
            var modelMappings = Assembly.Load("Shrooms.Presentation.ModelMappings");

            builder.RegisterApiControllers(shroomsApi);
            builder.RegisterApiControllers(premiumApi);
            builder.RegisterApiControllers(common);

            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new SignalRContractResolver();
            var serializer = JsonSerializer.Create(settings);
            builder.RegisterInstance(serializer).As<JsonSerializer>();
            builder.RegisterHubs(common);

            builder.RegisterAssemblyTypes(shroomsApi).Where(t => typeof(IBackgroundWorker).IsAssignableFrom(t)).InstancePerDependency().AsSelf();
            builder.RegisterType<AsyncRunner>().As<IAsyncRunner>().SingleInstance();
            builder.RegisterWebApiModelBinderProvider();
            builder.RegisterWebApiFilterProvider(config);
            builder.RegisterAssemblyTypes(dataLayer);
            builder.RegisterAssemblyTypes(modelMappings).AssignableTo(typeof(Profile)).As<Profile>();

            builder.RegisterAssemblyTypes(premiumApi).AssignableTo(typeof(Profile)).As<Profile>();

            // Interceptor
            builder.Register(_ => new TelemetryLoggingInterceptor());

            builder.RegisterType(typeof(UnitOfWork2)).As(typeof(IUnitOfWork2)).InstancePerRequest();

            builder.Register(c => HttpContext.Current == null ? new ShroomsDbContext(c.Resolve<ITenantNameContainer>().TenantName) : new ShroomsDbContext(getConnectionStringName()))
                .As<IDbContext>().InstancePerRequest();

            builder.RegisterType(typeof(EfUnitOfWork)).As(typeof(IUnitOfWork)).InstancePerRequest();
            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>));

            // Authorization types
            builder.RegisterType<MailingService>().As<IMailingService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<SmtpService>().As<IMailSendingService>().SingleInstance().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<PostNotificationService>().As<IPostNotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<CommentNotificationService>().As<ICommentNotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.Register(_ => app.GetDataProtectionProvider()).InstancePerRequest();
            builder.RegisterType<PermissionService>().As<IPermissionService>().PropertiesAutowired().InstancePerRequest();
            builder.RegisterType<SyncTokenService>().As<ISyncTokenService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<ImpersonateService>().As<IImpersonateService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<UserAdministrationValidator>().As<IUserAdministrationValidator>().InstancePerRequest();
            builder.RegisterType<OrganizationService>().As<IOrganizationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<ProjectsService>().As<IProjectsService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();

            builder.RegisterModule(new IdentityModule());
            builder.RegisterModule(new Modules.ServicesModule());
            builder.RegisterModule(new InfrastructureModule());
            builder.RegisterModule(new WallModule());
            builder.RegisterModule(new KudosModule());
            builder.RegisterModule(new KudosBasketModule());
            builder.RegisterModule(new WebHookCallbacksModule());
            builder.RegisterModule(new RefreshTokenModule());
            builder.RegisterModule(new ExternalLinksModule());
            builder.RegisterModule(new RoleModule());
            builder.RegisterModule(new MonitorsModule());
            builder.RegisterModule(new SupportModule());
            builder.RegisterModule(new AdministrationUsers());
            builder.RegisterModule(new JobModule());
            builder.RegisterModule(new FilterPresetModule());
            builder.RegisterModule(new BlacklistUserModule());
            builder.RegisterModule(new EmployeeModule());
            builder.RegisterModule(new WidgetModule());

            builder.RegisterModule(new BackgroundWorkersModule());
            builder.RegisterModule(new BadgesModule());
            builder.RegisterModule(new BooksModule());
            builder.RegisterModule(new CommitteeModule());
            builder.RegisterModule(new EventsModule());
            builder.RegisterModule(new KudosShopModule());
            builder.RegisterModule(new LotteryModule());
            builder.RegisterModule(new OfficeMapModule());
            builder.RegisterModule(new OrganizationalStructureModule());
            builder.RegisterModule(new ServiceRequestModule());
            builder.RegisterModule(new Premium.IoC.Modules.ServicesModule());
            builder.RegisterModule(new UserEventsModule());
            builder.RegisterModule(new VacationModule());
            builder.RegisterModule(new WebHookCallbacksPremiumModule());

            RegisterMapper(builder);

            var container = builder.Build();
            GlobalConfiguration.Configuration.UseAutofacActivator(container);

            return container;
        }


        private static void RegisterMapper(ContainerBuilder builder)
        {
            builder.Register(c => new MapperConfiguration(cfg =>
                {
                    foreach (var profile in c.Resolve<IEnumerable<Profile>>())
                    {
                        cfg.AddProfile(profile);
                    }
                }))
                .AsSelf().SingleInstance();

            builder.Register(c => c.Resolve<MapperConfiguration>()
                    .CreateMapper(c.Resolve))
                .As<IMapper>()
                .SingleInstance();
        }
    }
}
