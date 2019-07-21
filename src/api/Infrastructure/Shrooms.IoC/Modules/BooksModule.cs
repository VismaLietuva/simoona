using Autofac;
using DomainServiceValidators.Validators.Books;
using Shrooms.Domain.Services.Books;
using Shrooms.Domain.Services.Email.Book;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class BooksModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BookService>().As<IBookService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<BookServiceValidator>().As<IBookServiceValidator>().InstancePerRequest();
            builder.RegisterType<BookMobileServiceValidator>().As<IBookMobileServiceValidator>().InstancePerRequest();
            builder.RegisterType<BooksNotificationService>().As<IBooksNotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
