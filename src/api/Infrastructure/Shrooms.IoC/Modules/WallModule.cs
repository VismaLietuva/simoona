using Autofac;
using DomainServiceValidators.Validators.Wall;
using Shrooms.Domain.Services.Birthday;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Domain.Services.Wall.Posts.Comments;

namespace Shrooms.IoC.Modules
{
    public class WallModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PostService>().As<IPostService>().InstancePerRequest();
            builder.RegisterType<CommentService>().As<ICommentService>().InstancePerRequest();

            builder.RegisterType<WallService>().As<IWallService>().InstancePerRequest();
            builder.RegisterType<WallValidator>().As<IWallValidator>().InstancePerRequest();

            builder.RegisterType<BirthdayService>().As<IBirthdayService>().InstancePerRequest();
        }
    }
}
