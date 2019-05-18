using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Books;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
{
    public class BooksModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BookMobileService>().As<IBookMobileService>().InstancePerRequest();
        }
    }
}
