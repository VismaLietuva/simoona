using Autofac;
using Shrooms.Domain.Services.Books;
using Shrooms.Domain.Services.Email.Book;
using Shrooms.DomainServiceValidators.Validators.Books;
using Shrooms.Infrastructure.Books;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
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
