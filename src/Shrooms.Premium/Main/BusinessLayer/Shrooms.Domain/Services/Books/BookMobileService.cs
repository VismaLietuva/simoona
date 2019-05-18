using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DomainServiceValidators.Validators.Books;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Books;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Infrastructure.GoogleBookApiService;
using Shrooms.Infrastructure.GoogleBookService;

namespace Shrooms.Domain.Services.Books
{
    public class BookMobileService : IBookMobileService
    {
        private const int oneBook = 1;
        private const int oneMonth = 1;

        private static object newBookLock = new object();

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
        public IEnumerable<MobileUserDTO> GetUsersForAutoComplete(string search, int organizationId)
        {
            var searchLowerCase = search;

            var users = _userDbSet
                .Where(u => u.OrganizationId == organizationId)
                .Where(u => u.FirstName.StartsWith(searchLowerCase)
                    || u.LastName.StartsWith(searchLowerCase)
                    || u.UserName.StartsWith(searchLowerCase)
                    || u.Email.StartsWith(searchLowerCase)
                    || string.Concat(u.FirstName, " ", u.LastName).StartsWith(searchLowerCase))
                .OrderBy(u => u.FirstName)
                .Select(MapUsersToMobileUserDTO())
                .ToList();

            return users;
        }

        public IEnumerable<OfficeBookDTO> GetOffices(int organizationId)
        {
            var offices = _officeDbSet
                .Where(o => o.OrganizationId == organizationId)
                .Select(o => new OfficeBookDTO { Id = o.Id, Name = o.Name })
                .ToList();

            return offices;
        }

        public RetrievedBookInfoDTO GetBook(BookMobileGetDTO bookDTO)
        {
            var book = _bookOfficeDbSet
                .Include(b => b.Book)
                .Where(b => b.Book.Code == bookDTO.Code
                    && b.OfficeId == bookDTO.OfficeId
                    && b.OrganizationId == bookDTO.OrganizationId)
                .Select(MapBookToBookMobileGetDTO())
                .FirstOrDefault();

            _serviceValidator.ThrowIfBookDoesNotExist(book != null);

            return book;
        }

        public async Task<RetrievedBookInfoDTO> GetBookForPostAsync(string code, int organizationId)
        {
            var book = _bookDbSet
                .Where(b => b.Code == code
                    && b.OrganizationId == organizationId)
                .Select(MapBookToBookMobileGetForPostDTO())
                .FirstOrDefault();

            if (book == null)
            {
                book = await GetInfoFromExternalApiAsync(code);
            }

            return book;
        }

        public void PostBook(BookMobilePostDTO bookDTO)
        {
            lock (newBookLock)
            {
                var book = _bookDbSet
                    .Include(b => b.BookOffices)
                    .FirstOrDefault(b => b.Code == bookDTO.Code && b.OrganizationId == bookDTO.OrganizationId);

                if (book == null)
                {
                    AddNewBook(bookDTO);
                }
                else
                {
                    ValidatePostBook(bookDTO, book);
                    AddBookToOtherOffice(bookDTO, book);
                }

                _uow.SaveChanges(false);
            }
        }

        public IEnumerable<BookMobileLogDTO> ReturnBook(BookMobileReturnDTO bookDTO)
        {
            var booksToReturn = _bookLogDbSet
                 .Include(b => b.BookOffice)
                 .Include(b => b.BookOffice.Book)
                 .Include(b => b.ApplicationUser)
                 .Where(b => b.BookOffice.Book.Code == bookDTO.Code
                     && b.OrganizationId == bookDTO.OrganizationId
                     && b.Returned == null
                     && b.BookOffice.OfficeId == bookDTO.OfficeId)
                 .Select(MapBookToBookMobileLogDTO())
                 .ToList();

            _serviceValidator.ThrowIfThereIsNoBookToReturn(booksToReturn.Any());

            if (booksToReturn.Count() > 1)
            {
                return booksToReturn;
            }

            ReturnSpecificBook(booksToReturn.First().LogId);
            return null;
        }

        public void ReturnSpecificBook(int bookLogId)
        {
            var log = _bookLogDbSet.FirstOrDefault(l => l.Id == bookLogId && l.Returned == null);

            _serviceValidator.ThrowIfBookDoesNotExist(log != null);

            log.Returned = DateTime.UtcNow;
            log.Modified = DateTime.UtcNow;

            _uow.SaveChanges(false);
        }

        private Expression<Func<ApplicationUser, MobileUserDTO>> MapUsersToMobileUserDTO()
        {
            return user => new MobileUserDTO
            {
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName
            };
        }

        private Expression<Func<Book, RetrievedBookInfoDTO>> MapBookToBookMobileGetForPostDTO()
        {
            return book => new RetrievedBookInfoDTO()
            {
                Author = book.Author,
                Url = book.Url,
                Title = book.Title
            };
        }

        private async Task<RetrievedBookInfoDTO> GetInfoFromExternalApiAsync(string code)
        {
            var bookInfo = await _bookInfoService.FindBookByIsbnAsync(code);
            _serviceValidator.ThrowIfBookDoesNotExistGoogleAPI(bookInfo != null);
            return MapGoogleApiBookToDTO(bookInfo);
        }

        private RetrievedBookInfoDTO MapGoogleApiBookToDTO(ExternalBookInfo bookInfo)
        {
            return new RetrievedBookInfoDTO()
            {
                Author = bookInfo.Author,
                Title = bookInfo.Title,
                Url = bookInfo.Url
            };
        }

        private Expression<Func<BookOffice, RetrievedBookInfoDTO>> MapBookToBookMobileGetDTO()
        {
            return book => new RetrievedBookInfoDTO()
            {
                Author = book.Book.Author,
                Url = book.Book.Url,
                Title = book.Book.Title,
                BookOfficeId = book.Id
            };
        }

        private void ValidatePostBook(BookMobilePostDTO bookDTO, Book book)
        {
            var bookExistsInChosenOffice = book
                                .BookOffices
                                .Any((l => l.OfficeId == bookDTO.OfficeId));

            _serviceValidator.ThrowIfBookExist(bookExistsInChosenOffice);

            var officeExists = _officeDbSet.Any(o => o.Id == bookDTO.OfficeId);

            _serviceValidator.ThrowIfOfficeDoesNotExist(officeExists);
        }

        private void AddNewBook(BookMobilePostDTO bookDTO)
        {
            var newBook = new Book()
            {
                Title = bookDTO.Title,
                Author = bookDTO.Author,
                Code = bookDTO.Code,
                Url = bookDTO.Url,
                OrganizationId = bookDTO.OrganizationId,
                CreatedBy = bookDTO.ApplicationUserId,
                ModifiedBy = bookDTO.ApplicationUserId,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
            };

            _bookDbSet.Add(newBook);

            var newBookOffice = new BookOffice()
            {
                Book = newBook,
                OfficeId = bookDTO.OfficeId,
                OrganizationId = bookDTO.OrganizationId,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                CreatedBy = bookDTO.ApplicationUserId,
                ModifiedBy = bookDTO.ApplicationUserId,
                Quantity = oneBook,
            };

            _bookOfficeDbSet.Add(newBookOffice);
        }

        private void AddBookToOtherOffice(BookMobilePostDTO bookDTO, Book book)
        {
            var newBookOffice = new BookOffice()
            {
                BookId = book.Id,
                OfficeId = bookDTO.OfficeId,
                OrganizationId = bookDTO.OrganizationId,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                CreatedBy = bookDTO.ApplicationUserId,
                ModifiedBy = bookDTO.ApplicationUserId,
                Quantity = oneBook
            };

            _bookOfficeDbSet.Add(newBookOffice);
        }

        private Expression<Func<BookLog, BookMobileLogDTO>> MapBookToBookMobileLogDTO()
        {
            return log => new BookMobileLogDTO()
            {
                LogId = log.Id,
                UserFullName = log.ApplicationUser.FirstName + " " + log.ApplicationUser.LastName
            };
        }
    }
}
