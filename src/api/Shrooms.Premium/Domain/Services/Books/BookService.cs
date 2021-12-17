using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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

        private static readonly SemaphoreSlim _newBookLock = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim _takeBookLock = new SemaphoreSlim(1, 1);

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

        public BookService(IUnitOfWork2 uow,
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

        public async Task<ILazyPaged<BooksByOfficeDto>> GetBooksByOfficeAsync(BooksByOfficeOptionsDto options)
        {
            var allBooks = _bookOfficesDbSet
                .Include(x => x.Book)
                .Include(x => x.BookLogs.Select(v => v.ApplicationUser))
                .Where(x => x.OrganizationId == options.OrganizationId && x.Quantity != 0)
                .Where(OfficeFilter(options.OfficeId))
                .Where(SearchFilter(options.SearchString))
                .OrderBy(x => x.Book.Title)
                .Select(MapBooksWithReadersToDto(options.UserId));

            var totalBooksCount = await allBooks.CountAsync();
            var entriesCountToSkip = EntriesCountToSkip(options.Page);
            var books = await allBooks
                .Skip(() => entriesCountToSkip)
                .Take(() => BusinessLayerConstants.BooksPerPage)
                .ToListAsync();

            var pageDto = new LazyPaged<BooksByOfficeDto>(books, options.Page, BusinessLayerConstants.BooksPerPage, totalBooksCount);
            return pageDto;
        }

        public async Task<BookDetailsDto> GetBookDetailsAsync(int bookOfficeId, UserAndOrganizationDto userOrg)
        {
            var bookOffice = await _bookOfficesDbSet
                .Include(x => x.Book)
                .Include(x => x.BookLogs.Select(u => u.ApplicationUser))
                .Where(x => x.Id == bookOfficeId && x.OrganizationId == userOrg.OrganizationId)
                .Select(MapBookToDto())
                .FirstAsync();

            bookOffice.BookLogs = bookOffice
                .BookLogs
                .OrderByDescending(x => x.TakenFrom)
                .ToList();

            return bookOffice;
        }

        public async Task<BookDetailsAdministrationDto> GetBookDetailsWithOfficesAsync(int bookOfficeId, UserAndOrganizationDto userOrg)
        {
            var bookDetailsAdmin = new BookDetailsAdministrationDto
            {
                BookDetails = await GetBookDetailsAsync(bookOfficeId, userOrg)
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

        private static Expression<Func<BookOffice, BookQuantityByOfficeDto>> MapBookOfficeQuantitiesToDto()
        {
            return x => new BookQuantityByOfficeDto
            {
                BookQuantity = x.Quantity,
                OfficeId = x.OfficeId,
                OfficeName = x.Office.Name
            };
        }

        public async Task DeleteBookAsync(int bookId, UserAndOrganizationDto userOrg)
        {
            var bookOffices = await _bookOfficesDbSet
                .Include(x => x.Book)
                .Include(x => x.BookLogs)
                .Where(x => x.BookId == bookId && x.OrganizationId == userOrg.OrganizationId)
                .ToListAsync();

            _bookServiceValidator.CheckIfBookOfficesFoundWhileDeleting(bookOffices.Any());

            UpdateMetaFields(userOrg, bookOffices);
            await _uow.SaveChangesAsync(false);

            RemoveBookRelatedEntities(bookOffices);
            await _uow.SaveChangesAsync(false);
        }

        public async Task<RetrievedBookInfoDto> FindBookByIsbnAsync(string isbn, int organizationId)
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

        public async Task TakeBookAsync(int bookOfficeId, UserAndOrganizationDto userAndOrg)
        {
            var bookDto = new BookTakeDto
            {
                ApplicationUserId = userAndOrg.UserId,
                BookOfficeId = bookOfficeId,
                OrganizationId = userAndOrg.OrganizationId
            };

            await TakeBookAsync(bookDto);
        }

        public async Task TakeBookAsync(BookTakeDto bookDto)
        {
            MobileBookOfficeLogsDto officeBookWithLogs;

            await _takeBookLock.WaitAsync();

            try
            {
                officeBookWithLogs = await _bookOfficesDbSet
                    .Include(b => b.Book)
                    .Include(b => b.BookLogs)
                    .Where(b => b.OrganizationId == bookDto.OrganizationId && b.Id == bookDto.BookOfficeId)
                    .Select(MapOfficeBookWithLogsToDto())
                    .FirstOrDefaultAsync();

                await ValidateTakeBookAsync(bookDto, officeBookWithLogs);
                await BorrowBookAsync(officeBookWithLogs, bookDto);
            }
            finally
            {
                _takeBookLock.Release();
            }

            var book = new TakenBookDto
            {
                UserId = bookDto.ApplicationUserId,
                OrganizationId = bookDto.OrganizationId,
                BookOfficeId = bookDto.BookOfficeId
            };

            if (officeBookWithLogs != null)
            {
                book.OfficeId = officeBookWithLogs.OfficeId;
                book.Author = officeBookWithLogs.Author;
                book.Title = officeBookWithLogs.Title;
            }

            _asyncRunner.Run<IBooksNotificationService>(async notifier => await notifier.SendEmailAsync(book), _uow.ConnectionName);
        }

        public async Task ReturnBookAsync(int bookOfficeId, UserAndOrganizationDto userAndOrg)
        {
            var log = await _bookLogsDbSet
                .FirstOrDefaultAsync(l => l.BookOfficeId == bookOfficeId
                                          && l.ApplicationUserId == userAndOrg.UserId
                                          && l.OrganizationId == userAndOrg.OrganizationId
                                          && l.Returned == null);

            _bookServiceValidator.ThrowIfBookCannotBeReturned(log != null);

            // ReSharper disable once PossibleNullReferenceException
            log.Returned = DateTime.UtcNow;
            log.Modified = DateTime.UtcNow;
            log.ModifiedBy = userAndOrg.UserId;

            await _uow.SaveChangesAsync(false);
        }

        public async Task ReportBookAsync(BookReportDto bookReport, UserAndOrganizationDto userAndOrg)
        {
            var reportedOfficeBook = await _bookOfficesDbSet
                .Include(p => p.Book)
                .FirstAsync(p => p.Id == bookReport.BookOfficeId);

            var user = await _userService.GetApplicationUserAsync(userAndOrg.UserId);
            var receivers = await _roleService.GetAdministrationRoleEmailsAsync(userAndOrg.OrganizationId);

            var organization = await _organizationService.GetOrganizationByIdAsync(userAndOrg.OrganizationId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var bookUrl = _appSettings.BookUrl(organization.ShortName, bookReport.BookOfficeId, reportedOfficeBook.OfficeId);

            var subject = $"Reported book: {reportedOfficeBook.Book.Title}";
            var bookReportTemplateViewModel = new BookReportEmailTemplateViewModel(reportedOfficeBook.Book.Title, reportedOfficeBook.Book.Author,
                bookReport.Report, bookReport.Comment, bookUrl, user.FullName, userNotificationSettingsUrl);

            var content = _mailTemplate.Generate(bookReportTemplateViewModel, EmailPremiumTemplateCacheKeys.BookReport);
            var emailData = new EmailDto(receivers, subject, content);

            await _mailingService.SendEmailAsync(emailData);
        }

        public async Task AddBookAsync(NewBookDto bookDto)
        {
            await _newBookLock.WaitAsync();

            try
            {
                var bookAlreadyExists = await _booksDbSet
                    .AnyAsync(book =>
                        (bookDto.Isbn != null && book.Code == bookDto.Isbn && book.OrganizationId == bookDto.OrganizationId) ||
                        (bookDto.Isbn == null && book.OrganizationId == bookDto.OrganizationId && book.Title == bookDto.Title));

                _bookServiceValidator.CheckIfBookAlreadyExists(bookAlreadyExists);
                await ValidateQuantifiedOfficesAsync(bookDto.QuantityByOffice.Select(o => o.OfficeId));
                ValidateQuantitiesValues(bookDto.QuantityByOffice.Select(o => o.BookQuantity));

                var newBook = MapNewBookToEntity(bookDto);
                _booksDbSet.Add(newBook);

                bookDto.QuantityByOffice
                    .Where(office => office.BookQuantity > BookQuantityZero)
                    .ForEach(office => _bookOfficesDbSet.Add(MapBookDtoToBookOfficeEntity(newBook, office, bookDto.UserId)));

                await _uow.SaveChangesAsync(false);
            }
            finally
            {
                _newBookLock.Release();
            }
        }

        public async Task EditBookAsync(EditBookDto editedBook)
        {
            var existingBook = await _booksDbSet
                .Include(book => book.BookOffices)
                .FirstAsync(book =>
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

            await _uow.SaveChangesAsync(false);
        }

        public void UpdateBookCovers()
        {
            _asyncRunner.Run<IBookCoverService>(async service => await service.UpdateBookCoversAsync(), _uow.ConnectionName);
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

        private static void ManageQuantitiesInOffices(EditBookDto editedBook, Book existingBook)
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

        private static BookOffice MapBookDtoToBookOfficeEntity(Book newBook, NewBookQuantityDto quantity, string createdBy)
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

        private static Book MapNewBookToEntity(NewBookDto bookDto)
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

        private async Task ValidateQuantifiedOfficesAsync(IEnumerable<int> officesIds)
        {
            foreach (var officeId in officesIds)
            {
                _bookServiceValidator.CheckIfRequestedOfficesExist(await _officesDbSet.AnyAsync(o => o.Id == officeId));
            }
        }

        private void ValidateQuantitiesValues(IEnumerable<int> quantities)
        {
            if (quantities.Sum(bookQuantity => bookQuantity) <= 0)
            {
                _bookServiceValidator.CheckIfBookAllQuantitiesAreNotZero(false);
            }
        }

        private static void UpdateMetaFields(UserAndOrganizationDto userOrg, IEnumerable<BookOffice> bookOffices)
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

        private static Expression<Func<BookOffice, BookDetailsDto>> MapBookToDto()
        {
            return x => new BookDetailsDto
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
                BookLogs = x.BookLogs.Select(v => new BookDetailsLogDto
                {
                    UserId = v.ApplicationUserId,
                    LogId = v.Id,
                    FullName = v.ApplicationUser.FirstName + " " + v.ApplicationUser.LastName,
                    TakenFrom = v.TakenFrom,
                    Returned = v.Returned
                })
            };
        }

        private static Expression<Func<BookOffice, BooksByOfficeDto>> MapBooksWithReadersToDto(string userId)
        {
            return bookOffice => new BooksByOfficeDto
            {
                Id = bookOffice.Id,
                Author = bookOffice.Book.Author,
                Title = bookOffice.Book.Title,
                Url = bookOffice.Book.Url,
                QuantityLeft = bookOffice.Quantity - bookOffice.BookLogs.Count(x => x.Returned == null),
                OwnerId = bookOffice.Book.ApplicationUserId,
                Note = bookOffice.Book.Note,
                Readers = bookOffice.BookLogs.Where(x => x.Returned == null).Select(x => new BasicBookUserDto
                {
                    FullName = x.ApplicationUser.FirstName + " " + x.ApplicationUser.LastName,
                    Id = x.ApplicationUser.Id
                }),
                TakenByCurrentUser = bookOffice.BookLogs.Any(x => x.ApplicationUserId == userId && x.Returned == null)
            };
        }

        private static Expression<Func<BookOffice, bool>> OfficeFilter(int officeId)
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

        private static RetrievedBookInfoDto MapBookInfoToDto(ExternalBookInfo book)
        {
            var retrievedBookDto = new RetrievedBookInfoDto
            {
                Author = book.Author,
                Url = book.Url,
                Title = book.Title,
                OwnerId = book.OwnerId,
                Note = book.Note
            };
            return retrievedBookDto;
        }

        private static Expression<Func<BookOffice, MobileBookOfficeLogsDto>> MapOfficeBookWithLogsToDto()
        {
            return b => new MobileBookOfficeLogsDto
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

        private async Task ValidateTakeBookAsync(BookTakeDto bookDto, MobileBookOfficeLogsDto officeBookWithLogs)
        {
            var applicationUser = await _userDbSet.FirstOrDefaultAsync(u => u.Id == bookDto.ApplicationUserId);

            _serviceValidator.ThrowIfUserDoesNotExist(applicationUser);
            _serviceValidator.ThrowIfBookDoesNotExist(officeBookWithLogs != null);
            _serviceValidator.ChecksIfUserHasAlreadyBorrowedSameBook(officeBookWithLogs?.LogsUserIDs, bookDto.ApplicationUserId);
            _serviceValidator.ThrowIfBookIsAlreadyBorrowed(officeBookWithLogs);
        }

        private async Task BorrowBookAsync(MobileBookOfficeLogsDto officeBookWithLogs, BookTakeDto bookDto)
        {
            var bookLog = new BookLog
            {
                ApplicationUserId = bookDto.ApplicationUserId,
                BookOfficeId = officeBookWithLogs.BookOfficeId,
                ModifiedBy = bookDto.ApplicationUserId,
                Modified = DateTime.UtcNow,
                TakenFrom = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                CreatedBy = bookDto.ApplicationUserId,
                OrganizationId = bookDto.OrganizationId
            };

            _bookLogsDbSet.Add(bookLog);
            await _uow.SaveChangesAsync(false);
        }
    }
}
