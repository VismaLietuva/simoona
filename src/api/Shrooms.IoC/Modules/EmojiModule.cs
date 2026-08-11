using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Emoji;
using Shrooms.Domain.ServiceValidators.Validators.Emoji;

namespace Shrooms.IoC.Modules
{
    public static class EmojiModule
    {
        public static IServiceCollection AddCustomEmoji(this IServiceCollection services)
        {
            services.AddScoped<ICustomEmojiService, CustomEmojiService>();
            services.AddScoped<ICustomEmojiValidator, CustomEmojiValidator>();
            return services;
        }
    }
}
