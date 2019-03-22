using Autofac;
using Shrooms.Domain.Services.Books;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class BooksModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BookMobileService>().As<IBookMobileService>().InstancePerRequest();
        }
    }
}
