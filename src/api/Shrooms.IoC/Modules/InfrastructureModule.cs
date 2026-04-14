using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.DailyMailingService;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.CustomCache;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templating;
using Shrooms.Infrastructure.ExcelGenerator;
using Shrooms.Infrastructure.FeatureToggle;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.Storage;
using Shrooms.Infrastructure.Storage.AzureBlob;
using Shrooms.Infrastructure.Storage.FileSystem;
using Shrooms.Infrastructure.SystemClock;
using System.IO;

namespace Shrooms.IoC.Modules
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IMarkdownConverter, MarkdigMarkdownConverter>();
            services.AddScoped<IMailingService, MailingService>();
            services.AddSingleton(typeof(ICustomCache<,>), typeof(CustomCache<,>));
            services.AddScoped<IApplicationSettings, ApplicationSettings>();
            services.AddSingleton<ISystemClock, SystemClock>();
            services.AddScoped<IExcelBuilderFactory, ExcelBuilderFactory>();
            services.AddSingleton<IRazorLightEngine>(_ =>
                new RazorLightEngineBuilder()
                    .UseProject(new Shrooms.Infrastructure.Email.Templating.NormalizedFileSystemRazorProject(
                        Path.Combine(AppContext.BaseDirectory, "EmailTemplates")))
                    .UseMemoryCachingProvider()
                    .Build());
            services.AddScoped<IMailTemplate, MailTemplate>();
            services.AddScoped<IDailyMailingService, DailyMailingService>();
            services.AddScoped<IJobScheduler, HangFireScheduler>();
            services.AddSingleton<IFeatureConfiguration, AlwaysEnabledFeatureConfiguration>();

            RegisterStorage(services);

            return services;
        }

        private static void RegisterStorage(IServiceCollection services)
        {
            if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("StorageConnectionString")))
            {
                services.AddScoped<IStorage, FileSystemStorage>();
            }
            else
            {
                services.AddScoped<IStorage, AzureStorage>();
            }
        }
    }
}