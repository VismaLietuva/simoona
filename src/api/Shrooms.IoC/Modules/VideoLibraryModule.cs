using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.VideoLibrary;

namespace Shrooms.IoC.Modules
{
    public static class VideoLibraryModule
    {
        public static IServiceCollection AddVideoLibrary(this IServiceCollection services)
        {
            services.AddScoped<IVideoTypeService, VideoTypeService>();
            services.AddScoped<IVideoLibraryService, VideoLibraryService>();
            return services;
        }
    }
}
