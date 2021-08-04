using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.Premium.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Books;
using Shrooms.Premium.Domain.DomainServiceValidators.Books;
using Shrooms.Premium.Infrastructure.GoogleBookApiService;

namespace Shrooms.Premium.Domain.Services.Books
{
    public class BookMobileService : IBookMobileService
    {
        private const int OneBook = 1;

        private static readonly SemaphoreSlim _newBookLock = new SemaphoreSlim(1, 1);

        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Book> _bookDbSet;
        private readonly IDbSet<BookLog> _bookLogDbSet;
        private readonly IDbSet<Office> _officeDbSet;
        private readonly IDbSet<BookOffice> _bookOfficeDbSet;
        private readonly IDbSet<ApplicationUser> _userDbSet;
        private readonly IBookMobileServiceValidator _serviceValidator;
        private readonly IBookInfoService _bookInfoService;

        public BookMobileService(IUnitOfWork2 uow, IBookMobileServiceValidator serviceValidator, IBookInfoService bookInfoService)
        {
            _uow = uow;
            _bookDbSet = uow.GetDbSet<Book>();
            _bookLogDbSet = uow.GetDbSet<BookLog>();
            _officeDbSet = uow.GetDbSet<Office>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _bookOfficeDbSet = uow.GetDbSet<BookOffice>();
            _serviceValidator = serviceValidator;
            _bookInfoService = bookInfoService;
        }

        public async Task<IEnumerable<MobileUserDto>> GetUsersForAutoCompleteAsync(string search, int organizationId)
        {
            var searchLowerCase = search;

            var users = await _userDbSet
                .Where(u => u.OrganizationId == organizationId)
                .Where(u => u.FirstName.StartsWith(searchLowerCase)
                    || u.LastName.StartsWith(searchLowerCase)
                    || u.UserName.StartsWith(searchLowerCase)
                    || u.Email.StartsWith(searchLowerCase)
                    || string.Concat(u.FirstName, " ", u.LastName).StartsWith(searchLowerCase))
                .OrderBy(u => u.FirstName)
                .Select(MapUsersToMobileUserDto())
                .ToListAsync();

            return users;
        }

        public async Task<IEnumerable<OfficeBookDto>> GetOfficesAsync(int organizationId)
        {
            var offices = await _officeDbSet
                .Where(o => o.OrganizationId == organizationId)
                .Select(o => new OfficeBookDto { Id = o.Id, Name = o.Name })
                .ToListAsync();

            return offices;
        }

        public async Task<RetrievedBookInfoDto> GetBookAsync(BookMobileGetDto bookDto)
        {
            var book = await _bookOfficeDbSet
                .Include(b => b.Book)
                .Where(b => b.Book.Code == bookDto.Code
                    && b.OfficeId == bookDto.OfficeId
                    && b.OrganizationId == bookDto.OrganizationId)
                .Select(MapBookToBookMobileGetDto())
                .FirstOrDefaultAsync();

            _serviceValidator.ThrowIfBookDoesNotExist(book != null);

            return book;
        }

        public async Task<RetrievedBookInfoDto> GetBookForPostAsync(string code, int organizationId)
        {
            var book = _bookDbSet
                .Where(b => b.Code == code && b.OrganizationId == organizationId)
                .Select(MapBookToBookMobileGetForPostDto())
                .FirstOrDefault();

            if (book == null)
            {
                book = await GetInfoFromExternalApiAsync(code);
            }

            return book;
        }

        public async Task PostBookAsync(BookMobilePostDto bookDto)
        {
            await _newBookLock.WaitAsync();

            try
            {
                var book = await _bookDbSet
                    .Include(b => b.BookOffices)
                    .FirstOrDefaultAsync(b => b.Code == bookDto.Code && b.OrganizationId == bookDto.OrganizationId);

                if (book == null)
                {
                    AddNewBook(bookDto);
                }
                else
                {
                    await ValidatePostBookAsync(bookDto, book);
                    AddBookToOtherOffice(bookDto, book);
                }

                await _uow.SaveChangesAsync(false);
            }
            finally
            {
                _newBookLock.Release();
            }
        }

        public async Task<IEnumerable<BookMobileLogDto>> ReturnBookAsync(BookMobileReturnDto bookDto)
        {
            var booksToReturn = await _bookLogDbSet
                 .Include(b => b.BookOffice)
                 .Include(b => b.BookOffice.Book)
                 .Include(b => b.ApplicationUser)
                 .Where(b => b.BookOffice.Book.Code == bookDto.Code
                     && b.OrganizationId == bookDto.OrganizationId
                     && b.Returned == null
                     && b.BookOffice.OfficeId == bookDto.OfficeId)
                 .Select(MapBookToBookMobileLogDto())
                 .ToListAsync();

            _serviceValidator.ThrowIfThereIsNoBookToReturn(booksToReturn.Any());

            if (booksToReturn.Count > 1)
            {
                return booksToReturn;
            }

            await ReturnSpecificBookAsync(booksToReturn.First().LogId);
            return null;
        }

        public async Task ReturnSpecificBookAsync(int bookLogId)
        {
            var log = await _bookLogDbSet.FirstOrDefaultAsync(l => l.Id == bookLogId && l.Returned == null);

            _serviceValidator.ThrowIfBookDoesNotExist(log != null);

            // ReSharper disable once PossibleNullReferenceException
            log.Returned = DateTime.UtcNow;
            log.Modified = DateTime.UtcNow;

            await _uow.SaveChangesAsync(false);
        }

        private static Expression<Func<ApplicationUser, MobileUserDto>> MapUsersToMobileUserDto()
        {
            return user => new MobileUserDto
            {
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName
            };
        }

        private static Expression<Func<Book, RetrievedBookInfoDto>> MapBookToBookMobileGetForPostDto()
        {
            return book => new RetrievedBookInfoDto
            {
                Author = book.Author,
                Url = book.Url,
                Title = book.Title
            };
        }

        private async Task<RetrievedBookInfoDto> GetInfoFromExternalApiAsync(string code)
        {
            var bookInfo = await _bookInfoService.FindBookByIsbnAsync(code);
            _serviceValidator.ThrowIfBookDoesNotExistGoogleApi(bookInfo != null);
            return MapGoogleApiBookToDto(bookInfo);
        }

        private static RetrievedBookInfoDto MapGoogleApiBookToDto(ExternalBookInfo bookInfo)
        {
            return new RetrievedBookInfoDto
            {
                Author = bookInfo.Author,
                Title = bookInfo.Title,
                Url = bookInfo.Url
            };
        }

        private static Expression<Func<BookOffice, RetrievedBookInfoDto>> MapBookToBookMobileGetDto()
        {
            return book => new RetrievedBookInfoDto
            {
                Author = book.Book.Author,
                Url = book.Book.Url,
                Title = book.Book.Title,
                BookOfficeId = book.Id
            };
        }

        private async Task ValidatePostBookAsync(BookMobilePostDto bookDto, Book book)
        {
            var bookExistsInChosenOffice = book
                                .BookOffices
                                .Any((l => l.OfficeId == bookDto.OfficeId));

            _serviceValidator.ThrowIfBookExist(bookExistsInChosenOffice);

            var officeExists = await _officeDbSet.AnyAsync(o => o.Id == bookDto.OfficeId);

            _serviceValidator.ThrowIfOfficeDoesNotExist(officeExists);
        }

        private void AddNewBook(BookMobilePostDto bookDto)
        {
            var newBook = new Book
            {
                Title = bookDto.Title,
                Author = bookDto.Author,
                Code = bookDto.Code,
                Url = bookDto.Url,
                OrganizationId = bookDto.OrganizationId,
                CreatedBy = bookDto.ApplicationUserId,
                ModifiedBy = bookDto.ApplicationUserId,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };

            _bookDbSet.Add(newBook);

            var newBookOffice = new BookOffice
            {
                Book = newBook,
                OfficeId = bookDto.OfficeId,
                OrganizationId = bookDto.OrganizationId,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                CreatedBy = bookDto.ApplicationUserId,
                ModifiedBy = bookDto.ApplicationUserId,
                Quantity = OneBook
            };

            _bookOfficeDbSet.Add(newBookOffice);
        }

        private void AddBookToOtherOffice(BookMobilePostDto bookDto, Book book)
        {
            var newBookOffice = new BookOffice
            {
                BookId = book.Id,
                OfficeId = bookDto.OfficeId,
                OrganizationId = bookDto.OrganizationId,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                CreatedBy = bookDto.ApplicationUserId,
                ModifiedBy = bookDto.ApplicationUserId,
                Quantity = OneBook
            };

            _bookOfficeDbSet.Add(newBookOffice);
        }

        private static Expression<Func<BookLog, BookMobileLogDto>> MapBookToBookMobileLogDto()
        {
            return log => new BookMobileLogDto
            {
                LogId = log.Id,
                UserFullName = log.ApplicationUser.FirstName + " " + log.ApplicationUser.LastName
            };
        }
    }
}
