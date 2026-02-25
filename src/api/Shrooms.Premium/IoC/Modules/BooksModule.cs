using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.DomainServiceValidators.Books;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Domain.Services.Email.Book;

namespace Shrooms.Premium.IoC.Modules
{
    public static class BooksModule
    {
        public static IServiceCollection AddPremiumBooks(this IServiceCollection services)
        {
            services.AddScoped<IBookMobileService, BookMobileService>();
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IBookServiceValidator, BookServiceValidator>();
            services.AddScoped<IBookMobileServiceValidator, BookMobileServiceValidator>();
            services.AddScoped<IBooksNotificationService, BooksNotificationService>();
            services.AddScoped<IBookCoverService, BookCoverService>();
            return services;
        }
    }
}