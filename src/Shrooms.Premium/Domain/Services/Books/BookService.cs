using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MoreLinq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.UserService;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models;
using Shrooms.Premium.DataTransferObjects.Models.Books;
using Shrooms.Premium.DataTransferObjects.Models.Books.BookDetails;
using Shrooms.Premium.DataTransferObjects.Models.Books.BooksByOffice;
using Shrooms.Premium.DataTransferObjects.Models.LazyPaged;
using Shrooms.Premium.Domain.DomainServiceValidators.Books;
using Shrooms.Premium.Domain.Services.Email.Book;
using Shrooms.Premium.Infrastructure.GoogleBookApiService;

namespace Shrooms.Premium.Domain.Services.Books
{
    public class BookService : IBookService
    {
        private const int LastPage = 1;
        private const int BookQuantityZero = 0;
        private static object _newBookLock = new object();
        private static object _takeBookLock = new object();

        private readonly IUnitOfWork2 _uow;
        private readonly IApplicationSettings _appSettings;
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        private readonly IMailingService _mailingService;
        private readonly IMailTemplate _mailTemplate;
        private readonly IOrganizationService _organizationService;
        private readonly IBookInfoService _bookInfoService;
        private readonly IBookServiceValidator _bookServiceValidator;
        private readonly IBookMobileServiceValidator _serviceValidator;
        private readonly IAsyncRunner _asyncRunner;
        private readonly IDbSet<Book> _booksDbSet;
        private readonly IDbSet<Office> _officesDbSet;
        private readonly IDbSet<BookLog> _bookLogsDbSet;
        private readonly IDbSet<ApplicationUser> _userDbSet;
        private readonly IDbSet<BookOffice> _bookOfficesDbSet;

        public BookService(
            IUnitOfWork2 uow,
            IApplicationSettings appSettings,
            IRoleService roleService,
            IUserService userService,
            IMailingService mailingService,
            IMailTemplate mailTemplate,
            IOrganizationService organizationService,
            IBookInfoService bookInfoService,
            IBookServiceValidator bookServiceValidator,
            IBookMobileServiceValidator bookMobileServiceValidator,
            IAsyncRunner asyncRunner)
        {
            _uow = uow;
            _appSettings = appSettings;
            _roleService = roleService;
            _userService = userService;
            _mailingService = mailingService;
            _mailTemplate = mailTemplate;
            _organizationService = organizationService;
            _booksDbSet = uow.GetDbSet<Book>();
            _officesDbSet = uow.GetDbSet<Office>();
            _bookLogsDbSet = uow.GetDbSet<BookLog>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _bookOfficesDbSet = uow.GetDbSet<BookOffice>();

            _bookInfoService = bookInfoService;
            _bookServiceValidator = bookServiceValidator;
            _serviceValidator = bookMobileServiceValidator;
            _asyncRunner = asyncRunner;
        }

        public ILazyPaged<BooksByOfficeDTO> GetBooksByOffice(BooksByOfficeOptionsDTO options)
        {
            var allBooks = _bookOfficesDbSet
                .Include(x => x.Book)
                .Include(x => x.BookLogs.Select(v => v.ApplicationUser))
                .Where(x => x.OrganizationId == options.OrganizationId && x.Quantity != 0)
                .Where(OfficeFilter(options.OfficeId))
                .Where(SearchFilter(options.SearchString))
                .OrderBy(x => x.Book.Title)
                .Select(MapBooksWithReadersToDto(options.UserId));

            var totalBooksCount = allBooks.Count();
            var entriesCountToSkip = EntriesCountToSkip(options.Page);
            var books = allBooks
                .Skip(() => entriesCountToSkip)
                .Take(() => BusinessLayerConstants.BooksPerPage)
                .ToList();

            var pageDto = new LazyPaged<BooksByOfficeDTO>(books, options.Page, BusinessLayerConstants.BooksPerPage, totalBooksCount);
            return pageDto;
        }

        public BookDetailsDTO GetBookDetails(int bookOfficeId, UserAndOrganizationDTO userOrg)
        {
            var bookOffice = _bookOfficesDbSet
                .Include(x => x.Book)
                .Include(x => x.BookLogs.Select(u => u.ApplicationUser))
                .Where(x => x.Id == bookOfficeId && x.OrganizationId == userOrg.OrganizationId)
                .Select(MapBookToDto())
                .First();

            bookOffice.BookLogs = bookOffice
                .BookLogs
                .OrderByDescending(x => x.TakenFrom)
                .ToList();
            return bookOffice;
        }

        public BookDetailsAdministrationDTO GetBookDetailsWithOffices(int bookOfficeId, UserAndOrganizationDTO userOrg)
        {
            var bookDetailsAdmin = new BookDetailsAdministrationDTO
            {
                BookDetails = GetBookDetails(bookOfficeId, userOrg)
            };

            bookDetailsAdmin.QuantityByOffice = _bookOfficesDbSet
                .Include(x => x.Office)
                .Where(b =>
                    b.BookId == bookDetailsAdmin.BookDetails.Id &&
                    b.OrganizationId == userOrg.OrganizationId)
                .Select(MapBookOfficeQuantitiesToDto())
                .ToList();
            return bookDetailsAdmin;
        }

        private Expression<Func<BookOffice, BookQuantityByOfficeDTO>> MapBookOfficeQuantitiesToDto()
        {
            return x => new BookQuantityByOfficeDTO
            {
                BookQuantity = x.Quantity,
                OfficeId = x.OfficeId,
                OfficeName = x.Office.Name
            };
        }

        public void DeleteBook(int bookId, UserAndOrganizationDTO userOrg)
        {
            var bookOffices = _bookOfficesDbSet
                .Include(x => x.Book)
                .Include(x => x.BookLogs)
                .Where(x => x.BookId == bookId && x.OrganizationId == userOrg.OrganizationId)
                .ToList();

            _bookServiceValidator.CheckIfBookOfficesFoundWhileDeleting(bookOffices.Any());

            UpdateMetaFields(userOrg, bookOffices);
            _uow.SaveChanges(false);

            RemoveBookRelatedEntities(bookOffices);
            _uow.SaveChanges(false);
        }

        public async Task<RetrievedBookInfoDTO> FindBookByIsbn(string isbn, int organizationId)
        {
            var bookAlreadyExists = _booksDbSet
                .Any(book =>
                    book.OrganizationId == organizationId
                    && book.Code == isbn);

            _bookServiceValidator.CheckIfBookAlreadyExists(bookAlreadyExists);

            var result = await _bookInfoService.FindBookByIsbnAsync(isbn);

            _bookServiceValidator.CheckIfBookWasFoundByIsbnFromExternalProvider(result);
            var retrievedBookDto = MapBookInfoToDto(result);
            return retrievedBookDto;
        }

        public void TakeBook(int bookOfficeId, UserAndOrganizationDTO userAndOrg)
        {
            var bookDTO = new BookTakeDTO
            {
                ApplicationUserId = userAndOrg.UserId,
                BookOfficeId = bookOfficeId,
                OrganizationId = userAndOrg.OrganizationId
            };
            TakeBook(bookDTO);
        }

        public void TakeBook(BookTakeDTO bookDTO)
        {
            MobileBookOfficeLogsDTO officeBookWithLogs;
            lock (_takeBookLock)
            {
                officeBookWithLogs = _bookOfficesDbSet
                    .Include(b => b.Book)
                    .Include(b => b.BookLogs)
                    .Where(b => b.OrganizationId == bookDTO.OrganizationId
                        && b.Id == bookDTO.BookOfficeId)
                    .Select(MapOfficebookWithLogsToDTO())
                    .FirstOrDefault();

                ValidateTakeBook(bookDTO, officeBookWithLogs);

                BorrowBook(officeBookWithLogs, bookDTO);
            }

            var book = new TakenBookDTO
            {
                UserId = bookDTO.ApplicationUserId,
                OrganizationId = bookDTO.OrganizationId,
                BookOfficeId = bookDTO.BookOfficeId,
                OfficeId = officeBookWithLogs.OfficeId,
                Author = officeBookWithLogs.Author,
                Title = officeBookWithLogs.Title
            };
            _asyncRunner.Run<IBooksNotificationService>(n => n.SendEmail(book), _uow.ConnectionName);
        }

        public void ReturnBook(int bookOfficeId, UserAndOrganizationDTO userAndOrg)
        {
            var log = _bookLogsDbSet
                .FirstOrDefault(l => l.BookOfficeId == bookOfficeId
                    && l.ApplicationUserId == userAndOrg.UserId
                    && l.OrganizationId == userAndOrg.OrganizationId
                    && l.Returned == null);

            _bookServiceValidator.ThrowIfBookCannotBeReturned(log != null);

            log.Returned = DateTime.UtcNow;
            log.Modified = DateTime.UtcNow;
            log.ModifiedBy = userAndOrg.UserId;

            _uow.SaveChanges(false);
        }

        public void ReportBook(BookReportDTO bookReport, UserAndOrganizationDTO userAndOrg)
        {
            var reportedOfficeBook = _bookOfficesDbSet
                .Include(p => p.Book)
                .FirstOrDefault(p => p.Id == bookReport.BookOfficeId);

            var user = _userService.GetApplicationUser(userAndOrg.UserId);
            var receivers = _roleService.GetAdministrationRoleEmails(userAndOrg.OrganizationId);

            var organization = _organizationService.GetOrganizationById(userAndOrg.OrganizationId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var bookUrl = _appSettings.BookUrl(organization.ShortName, bookReport.BookOfficeId, reportedOfficeBook.OfficeId);
            var subject = $"Reported book: {reportedOfficeBook.Book.Title}";
            var bookReportTemplateViewModel = new BookReportEmailTemplateViewModel(reportedOfficeBook.Book.Title, reportedOfficeBook.Book.Author,
                 bookReport.Report, bookReport.Comment, bookUrl, user.FullName, userNotificationSettingsUrl);

            var content = _mailTemplate.Generate(bookReportTemplateViewModel, EmailPremiumTemplateCacheKeys.BookReport);
            var emailData = new EmailDto(receivers, subject, content);

            _mailingService.SendEmail(emailData);
        }

        public void AddBook(NewBookDTO bookDto)
        {
            lock (_newBookLock)
            {
                var bookAlreadyExists = _booksDbSet
                    .Any(book =>
                        (bookDto.Isbn != null &&
                         book.Code == bookDto.Isbn &&
                         book.OrganizationId == bookDto.OrganizationId) ||
                        (bookDto.Isbn == null &&
                         book.OrganizationId == bookDto.OrganizationId &&
                         book.Title == bookDto.Title));

                _bookServiceValidator.CheckIfBookAlreadyExists(bookAlreadyExists);
                ValidateQuantifiedOffices(bookDto.QuantityByOffice.Select(o => o.OfficeId));
                ValidateQuantitiesValues(bookDto.QuantityByOffice.Select(o => o.BookQuantity));

                var newBook = MapNewBookToEntity(bookDto);
                _booksDbSet.Add(newBook);

                bookDto.QuantityByOffice
                    .Where(office => office.BookQuantity > BookQuantityZero)
                    .ForEach(office => _bookOfficesDbSet.Add(MapBookDtoToBookOfficeEntity(newBook, office, bookDto.UserId)));

                _uow.SaveChanges(false);
            }
        }

        public void EditBook(EditBookDTO editedBook)
        {
            var existingBook = _booksDbSet
                .Include(book => book.BookOffices)
                .First(book =>
                    book.Id == editedBook.Id &&
                    book.OrganizationId == editedBook.OrganizationId);

            existingBook.Modified = DateTime.UtcNow;
            existingBook.ModifiedBy = editedBook.UserId;
            existingBook.Title = editedBook.Title;
            existingBook.Author = editedBook.Author;
            existingBook.Code = editedBook.Isbn;
            existingBook.Url = editedBook.Url;
            existingBook.ApplicationUserId = editedBook.OwnerId;
            existingBook.Note = editedBook.Note;

            ValidateQuantitiesValues(editedBook.QuantityByOffice.Select(o => o.BookQuantity));
            ManageQuantitiesInOffices(editedBook, existingBook);
            _uow.SaveChanges(false);
        }

        public void UpdateBookCovers()
        {
            _asyncRunner.Run<IBookCoverService>(service => service.UpdateBookCovers(), _uow.ConnectionName);
        }

        private static Expression<Func<BookOffice, bool>> SearchFilter(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                return x => true;
            }

            return x =>
                x.Book.Author.Contains(searchString) ||
                x.Book.Title.Contains(searchString) ||
                x.BookLogs.Any(v =>
                    (v.ApplicationUser.FirstName.Contains(searchString) ||
                    v.ApplicationUser.LastName.Contains(searchString)) &&
                    v.Returned == null);
        }

        private void ManageQuantitiesInOffices(EditBookDTO editedBook, Book existingBook)
        {
            foreach (var officeQuantity in editedBook.QuantityByOffice)
            {
                var bookOfficeExists = existingBook.BookOffices.Any(x => x.OfficeId == officeQuantity.OfficeId);
                if (!bookOfficeExists)
                {
                    existingBook.BookOffices.Add(MapBookDtoToBookOfficeEntity(existingBook, officeQuantity, editedBook.UserId));
                }
                else
                {
                    var office = existingBook.BookOffices.First(x => x.OfficeId == officeQuantity.OfficeId);
                    office.Quantity = officeQuantity.BookQuantity;
                }
            }
        }

        private BookOffice MapBookDtoToBookOfficeEntity(Book newBook, NewBookQuantityDTO quantity, string createdBy)
        {
            return new BookOffice
            {
                BookId = newBook.Id,
                Created = DateTime.UtcNow,
                CreatedBy = createdBy,
                Modified = DateTime.UtcNow,
                OfficeId = quantity.OfficeId,
                OrganizationId = newBook.OrganizationId,
                Quantity = quantity.BookQuantity
            };
        }

        private Book MapNewBookToEntity(NewBookDTO bookDto)
        {
            return new Book
            {
                Author = bookDto.Author,
                Code = bookDto.Isbn,
                Created = DateTime.UtcNow,
                CreatedBy = bookDto.UserId,
                Modified = DateTime.UtcNow,
                OrganizationId = bookDto.OrganizationId,
                Title = bookDto.Title,
                Url = bookDto.Url,
                ApplicationUserId = bookDto.OwnerId,
                Note = bookDto.Note
            };
        }

        private void ValidateQuantifiedOffices(IEnumerable<int> officesIds)
        {
            officesIds.ForEach(officeId =>
                _bookServiceValidator.CheckIfRequestedOfficesExist(_officesDbSet.Any(o => o.Id == officeId)));
        }

        private void ValidateQuantitiesValues(IEnumerable<int> quantities)
        {
            if (quantities.Sum(bookQuantity => bookQuantity) <= 0)
            {
                _bookServiceValidator.CheckIfBookAllQuantitiesAreNotZero(false);
            }
        }

        private void UpdateMetaFields(UserAndOrganizationDTO userOrg, IEnumerable<BookOffice> bookOffices)
        {
            bookOffices.ForEach(bookOffice =>
            {
                bookOffice.BookLogs.ForEach(log =>
                {
                    log.Modified = DateTime.UtcNow;
                    log.ModifiedBy = userOrg.UserId;
                });

                bookOffice.Book.Modified = DateTime.UtcNow;
                bookOffice.Book.ModifiedBy = userOrg.UserId;
                bookOffice.Modified = DateTime.UtcNow;
                bookOffice.ModifiedBy = userOrg.UserId;
            });
        }

        private void RemoveBookRelatedEntities(IList<BookOffice> bookOffices)
        {
            var bookToRemove = bookOffices.First().Book;
            bookOffices.ToList().ForEach(bookOffice =>
            {
                bookOffice.BookLogs.ToList().ForEach(log => _bookLogsDbSet.Remove(log));
                _bookOfficesDbSet.Remove(bookOffice);
            });
            _booksDbSet.Remove(bookToRemove);
        }

        private Expression<Func<BookOffice, BookDetailsDTO>> MapBookToDto()
        {
            return x => new BookDetailsDTO
            {
                BookOfficeId = x.Id,
                Id = x.BookId,
                Author = x.Book.Author,
                Url = x.Book.Url,
                Isbn = x.Book.Code,
                Title = x.Book.Title,
                CanBeTaken = (x.Quantity - x.BookLogs.Count(v => v.Returned == null)) > BookQuantityZero,
                OwnerId = x.Book.ApplicationUserId,
                OwnerFullName = (x.Book.ApplicationUserId != null) ? x.Book.ApplicationUser.FirstName + " " + x.Book.ApplicationUser.LastName : null,
                Note = x.Book.Note,
                CoverUrl = x.Book.BookCoverUrl,
                BookLogs = x.BookLogs.Select(v => new BookDetailsLogDTO
                {
                    UserId = v.ApplicationUserId,
                    LogId = v.Id,
                    FullName = v.ApplicationUser.FirstName + " " + v.ApplicationUser.LastName,
                    TakenFrom = v.TakenFrom,
                    Returned = v.Returned
                })
            };
        }

        private Expression<Func<BookOffice, BooksByOfficeDTO>> MapBooksWithReadersToDto(string userId)
        {
            return bookOffice => new BooksByOfficeDTO
            {
                Id = bookOffice.Id,
                Author = bookOffice.Book.Author,
                Title = bookOffice.Book.Title,
                Url = bookOffice.Book.Url,
                QuantityLeft = bookOffice.Quantity - bookOffice.BookLogs.Count(x => x.Returned == null),
                OwnerId = bookOffice.Book.ApplicationUserId,
                Note = bookOffice.Book.Note,
                Readers = bookOffice.BookLogs.Where(x => x.Returned == null).Select(x => new BasicBookUserDTO
                {
                    FullName = x.ApplicationUser.FirstName + " " + x.ApplicationUser.LastName,
                    Id = x.ApplicationUser.Id
                }),
                TakenByCurrentUser = bookOffice.BookLogs.Any(x => x.ApplicationUserId == userId && x.Returned == null)
            };
        }

        private Expression<Func<BookOffice, bool>> OfficeFilter(int officeId)
        {
            if (officeId == 0)
            {
                return x => true;
            }

            return x => x.OfficeId == officeId;
        }

        private static int EntriesCountToSkip(int pageRequested)
        {
            return (pageRequested - LastPage) * BusinessLayerConstants.BooksPerPage;
        }

        private RetrievedBookInfoDTO MapBookInfoToDto(ExternalBookInfo book)
        {
            var retrievedBookDto = new RetrievedBookInfoDTO
            {
                Author = book.Author,
                Url = book.Url,
                Title = book.Title,
                OwnerId = book.OwnerId,
                Note = book.Note
            };
            return retrievedBookDto;
        }

        private Expression<Func<BookOffice, MobileBookOfficeLogsDTO>> MapOfficebookWithLogsToDTO()
        {
            return b => new MobileBookOfficeLogsDTO
            {
                BookOfficeId = b.Id,
                Quantity = b.Quantity,
                LogsUserIDs = b.BookLogs
                    .Where(x => x.Returned == null)
                    .Select(x => x.ApplicationUserId),
                Author = b.Book.Author,
                Title = b.Book.Title,
                BookId = b.BookId,
                OfficeId = b.OfficeId
            };
        }

        private void ValidateTakeBook(BookTakeDTO bookDTO, MobileBookOfficeLogsDTO officeBookWithLogs)
        {
            var applicationUser = _userDbSet.FirstOrDefault(u => u.Id == bookDTO.ApplicationUserId);

            _serviceValidator.ThrowIfUserDoesNotExist(applicationUser);
            _serviceValidator.ThrowIfBookDoesNotExist(officeBookWithLogs != null);
            _serviceValidator.ChecksIfUserHasAlreadyBorrowedSameBook(officeBookWithLogs?.LogsUserIDs, bookDTO.ApplicationUserId);
            _serviceValidator.ThrowIfBookIsAlreadyBorrowed(officeBookWithLogs);
        }

        private void BorrowBook(MobileBookOfficeLogsDTO officeBookWithLogs, BookTakeDTO bookDTO)
        {
            var bookLog = new BookLog
            {
                ApplicationUserId = bookDTO.ApplicationUserId,
                BookOfficeId = officeBookWithLogs.BookOfficeId,
                ModifiedBy = bookDTO.ApplicationUserId,
                Modified = DateTime.UtcNow,
                TakenFrom = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                CreatedBy = bookDTO.ApplicationUserId,
                OrganizationId = bookDTO.OrganizationId
            };

            _bookLogsDbSet.Add(bookLog);
            _uow.SaveChanges(false);
        }
    }
}
