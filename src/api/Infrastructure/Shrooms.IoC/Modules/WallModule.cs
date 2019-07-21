using Autofac;
using DomainServiceValidators.Validators.Wall;
using Shrooms.Domain.Services.Birthday;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class WallModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PostService>().As<IPostService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<CommentService>().As<ICommentService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<WallService>().As<IWallService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<WallValidator>().As<IWallValidator>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<BirthdayService>().As<IBirthdayService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
