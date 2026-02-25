using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Birthday;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.Domain.ServiceValidators.Validators.Wall;

namespace Shrooms.IoC.Modules
{
    public static class WallModule
    {
        public static IServiceCollection AddWall(this IServiceCollection services)
        {
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IWallService, WallService>();
            services.AddScoped<IWallValidator, WallValidator>();
            services.AddScoped<IBirthdayService, BirthdayService>();
            return services;
        }
    }
}
