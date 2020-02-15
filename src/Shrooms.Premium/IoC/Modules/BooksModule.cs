using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.DomainServiceValidators.Books;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Domain.Services.Email.Book;

namespace Shrooms.Premium.IoC.Modules
{
    public class BooksModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BookMobileService>().As<IBookMobileService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<BookService>().As<IBookService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<BookServiceValidator>().As<IBookServiceValidator>().InstancePerRequest();
            builder.RegisterType<BookMobileServiceValidator>().As<IBookMobileServiceValidator>().InstancePerRequest();
            builder.RegisterType<BooksNotificationService>().As<IBooksNotificationService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
            builder.RegisterType<BookCoverService>().As<IBookCoverService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
