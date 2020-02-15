using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.UserService;
using Shrooms.Premium.DataTransferObjects.Models.Books;
using Shrooms.Premium.DataTransferObjects.Models.Books.BookDetails;
using Shrooms.Premium.DataTransferObjects.Models.Books.BooksByOffice;
using Shrooms.Premium.Domain.DomainExceptions.Book;
using Shrooms.Premium.Domain.DomainServiceValidators.Books;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Infrastructure.GoogleBookApiService;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class BookServiceTests
    {
        private IBookService _bookService;
        private IApplicationSettings _appSettings;
        private IRoleService _roleService;
        private IUserService _userService;
        private IMailingService _mailingService;
        private IMailTemplate _mailTemplate;
        private IOrganizationService _organizationService;
        private IBookInfoService _bookInfoService;
        private IDbSet<BookOffice> _bookOfficesDbSet;
        private IDbSet<BookLog> _bookLogsDbSet;
        private BookServiceValidator _bookServiceValidator;
        private IDbSet<Book> _booksDbSet;
        private IDbSet<Office> _officesDbSet;
        private IDbSet<ApplicationUser> _userDbSet;
        private IBookMobileServiceValidator _validationService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _bookOfficesDbSet = Substitute.For<IDbSet<BookOffice>>();
            uow.GetDbSet<BookOffice>().Returns(_bookOfficesDbSet);

            _bookLogsDbSet = Substitute.For<IDbSet<BookLog>>();
            uow.GetDbSet<BookLog>().Returns(_bookLogsDbSet);

            _booksDbSet = Substitute.For<IDbSet<Book>>();
            uow.GetDbSet<Book>().Returns(_booksDbSet);

            _officesDbSet = Substitute.For<IDbSet<Office>>();
            uow.GetDbSet<Office>().Returns(_officesDbSet);

            _userDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            uow.GetDbSet<ApplicationUser>().Returns(_userDbSet);

            _appSettings = Substitute.For<IApplicationSettings>();
            _validationService = Substitute.For<IBookMobileServiceValidator>();
            _bookInfoService = Substitute.For<IBookInfoService>();
            _roleService = Substitute.For<IRoleService>();
            _userService = Substitute.For<IUserService>();
            _mailingService = Substitute.For<IMailingService>();
            _mailTemplate = Substitute.For<IMailTemplate>();
            _organizationService = Substitute.For<IOrganizationService>();
            _bookServiceValidator = new BookServiceValidator();
            var asyncRunner = Substitute.For<IAsyncRunner>();
            _bookService = new BookService(uow, _appSettings, _roleService, _userService, _mailingService, _mailTemplate, _organizationService, _bookInfoService, _bookServiceValidator, _validationService, asyncRunner);
        }

        [Test]
        public async Task Should_Return_Correctly_Mapped_Book_Info_By_Isbn()
        {
            MockBookRetrieval();
            var expected = new RetrievedBookInfoDTO
            {
                Author = "test",
                Url = "test",
                Title = "asd"
            };
            var result = await _bookService.FindBookByIsbn("123", 2);
            Assert.AreEqual(expected.Title, result.Title);
            Assert.AreEqual(expected.Url, result.Url);
            Assert.AreEqual(expected.Author, result.Author);
        }

        [Test]
        public void Should_Return_Books_By_Office()
        {
            MockBooksByOffice();
            var options = new BooksByOfficeOptionsDTO { OrganizationId = 2, OfficeId = 1, Page = 1, UserId = "testUserId" };
            var res = _bookService.GetBooksByOffice(options);
            Assert.AreEqual(res.ItemCount, 2);
            Assert.AreEqual(res.Entries.First().QuantityLeft, 1);
            Assert.AreEqual(res.Entries.First().Readers.First().Id, "testUserId");
            Assert.IsTrue(res.Entries.First().TakenByCurrentUser);
        }

        [Test]
        public void Should_Return_Correct_Books_By_Office_Search_Results()
        {
            MockBooksByOffice();
            var options = new BooksByOfficeOptionsDTO { OrganizationId = 2, OfficeId = 1, Page = 1, UserId = "testUserId", SearchString = "search" };
            var res = _bookService.GetBooksByOffice(options);
            Assert.AreEqual(res.ItemCount, 1);
            Assert.AreEqual(res.Entries.First().Title, "Test2search");
        }

        [Test]
        public void Should_Return_Correct_Books_Off_All_Offices()
        {
            MockBooksByOffice();
            var options = new BooksByOfficeOptionsDTO { OrganizationId = 2, Page = 1 };
            var res = _bookService.GetBooksByOffice(options);

            Assert.AreEqual(res.ItemCount, 3);
        }

        [Test]
        public void Should_Return_Correctly_Mapped_Book_Details_To_User()
        {
            MockGetBookDetails();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "testUser2"
            };
            var res = _bookService.GetBookDetails(2, userOrg);
            Assert.AreEqual(2, res.BookLogs.Count());
            Assert.AreEqual(2, res.BookOfficeId);
            Assert.AreEqual(1, res.Id);
            Assert.AreEqual(1, res.BookLogs.First().LogId);
            Assert.AreEqual("name1 surname1", res.BookLogs.First().FullName);
        }

        [Test]
        public void Should_Return_Correctly_Mapped_Book_Details_To_Administrator()
        {
            MockGetBookDetails();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "testUser2"
            };
            var res = _bookService.GetBookDetailsWithOffices(2, userOrg);
            Assert.AreEqual(2, res.QuantityByOffice.Count());
            Assert.IsTrue(res.QuantityByOffice.Any(x => x.OfficeId == 1));
            Assert.IsTrue(res.QuantityByOffice.Any(x => x.OfficeId == 2));
        }

        [Test]
        public void Should_Delete_Book_With_Its_Related_Entities()
        {
            MockDeleteBookEntities();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            _bookService.DeleteBook(1, userOrg);
            _bookOfficesDbSet.Received(2).Remove(Arg.Any<BookOffice>());
        }

        [Test]
        public void Should_Throw_Exception_If_Book_Already_Exists()
        {
            Assert.Throws<BookException>(() => _bookServiceValidator.CheckIfBookAlreadyExists(true));
        }

        [Test]
        public void Should_Not_Throw_If_Book_Does_Not_Exist()
        {
            Assert.DoesNotThrow(() => _bookServiceValidator.CheckIfBookAlreadyExists(false));
        }

        [Test]
        public void Should_Throw_While_Creating_Book_With_Wrong_Offices_Provided()
        {
            Assert.Throws<ArgumentException>(() => _bookServiceValidator.CheckIfRequestedOfficesExist(false));
        }

        [Test]
        public void Should_Not_Throw_While_Creating_Book_With_Correct_Offices_Provided()
        {
            Assert.DoesNotThrow(() => _bookServiceValidator.CheckIfRequestedOfficesExist(true));
        }

        [Test]
        public void Should_Throw_On_Add_Book_If_Incorrect_Office_Is_Provided()
        {
            _booksDbSet.SetDbSetData(new List<Book>());
            _officesDbSet.SetDbSetData(new List<Office>());
            var newBookDto = new NewBookDTO
            {
                Author = "test",
                Isbn = "123",
                OrganizationId = 2,
                Title = "test",
                QuantityByOffice = new List<NewBookQuantityDTO>
                {
                    new NewBookQuantityDTO
                    {
                        OfficeId = 1,
                        BookQuantity = 0
                    },
                    new NewBookQuantityDTO
                    {
                        OfficeId = 2,
                        BookQuantity = 0
                    }
                },
                UserId = "testUserId"
            };

            Assert.Throws<ArgumentException>(() => _bookService.AddBook(newBookDto));
        }

        [Test]
        public void Should_Throw_On_Add_Book_If_Quantity_Is_Zero()
        {
            _booksDbSet.SetDbSetData(new List<Book>());

            var offices = new List<Office>()
            {
                new Office()
                {
                    Id = 1
                }
            };
            _officesDbSet.SetDbSetData(offices);

            var newBookDto = new NewBookDTO
            {
                Author = "test",
                Isbn = "123",
                OrganizationId = 2,
                Title = "test",
                QuantityByOffice = new List<NewBookQuantityDTO>
                {
                    new NewBookQuantityDTO
                    {
                        OfficeId = 1,
                        BookQuantity = 0
                    }
                },
                UserId = "testUserId"
            };

            Assert.Throws<BookException>(() => _bookService.AddBook(newBookDto));
        }

        [Test]
        public void Should_Add_Correct_Data_To_Logs_On_Take_Book()
        {
            MockCreateNewBook();
            MockBooksByOffice();
            MockApplicationUsers();
            MockDeleteBookEntities();

            var takeBook = new BookTakeDTO()
            {
                ApplicationUserId = "testUser1",
                BookOfficeId = 1,
                OrganizationId = 2
            };

            _bookService.TakeBook(takeBook);

            _bookLogsDbSet.Received().Add(Arg.Is<BookLog>(b => b.OrganizationId == 2 && b.ApplicationUserId == "testUser1" && b.OrganizationId == 2));
        }

        [Test]
        public void Should_Throw_On_Take_Book_When_No_Users_Exist()
        {
            MockBooksByOffice();

            _userDbSet.SetDbSetData(new List<ApplicationUser>());
            var bookTake = new BookTakeDTO()
            {
                ApplicationUserId = "testUser1",
                BookOfficeId = 1,
                OrganizationId = 2
            };

            _validationService
                .When(x => x.ThrowIfUserDoesNotExist(null))
                .Do(x => { throw new BookException("ThrowIfUserDoesNotExist"); });

            Assert.Throws<BookException>(() => _bookService.TakeBook(bookTake));
        }

        [Test]
        public void Should_Create_New_Book_And_Book_Office()
        {
            MockCreateNewBook();
            var newBookDto = new NewBookDTO
            {
                Author = "test",
                Isbn = "123",
                OrganizationId = 2,
                Title = "test",
                QuantityByOffice = new List<NewBookQuantityDTO>
                {
                    new NewBookQuantityDTO
                    {
                        OfficeId = 1,
                        BookQuantity = 0
                    },
                    new NewBookQuantityDTO
                    {
                        OfficeId = 2,
                        BookQuantity = 5
                    }
                },
                UserId = "testUserId"
            };

            _bookService.AddBook(newBookDto);
            _booksDbSet.Received(1).Add(Arg.Any<Book>());
            _bookOfficesDbSet.Received(1).Add(Arg.Any<BookOffice>());
        }

        [Test]
        public void Should_Edit_Book_With_Quantities_In_Offices()
        {
            MockEditBook();
            var bookDto = new EditBookDTO
            {
                OrganizationId = 2,
                Author = "test1",
                Id = 1,
                QuantityByOffice = new List<NewBookQuantityDTO>
                {
                    new NewBookQuantityDTO
                    {
                        BookQuantity = 0,
                        OfficeId = 1
                    },
                    new NewBookQuantityDTO
                    {
                        BookQuantity = 50,
                        OfficeId = 2
                    }
                }
            };
            _bookService.EditBook(bookDto);
            var bookOffices = _booksDbSet.First().BookOffices;
            Assert.AreEqual(0, bookOffices.First(x => x.OfficeId == 1).Quantity);
            Assert.AreEqual(50, bookOffices.First(x => x.OfficeId == 2).Quantity);
            Assert.AreEqual("test1", _booksDbSet.First().Author);
        }

        #region Mocks
        private void MockCreateNewBook()
        {
            var offices = new List<Office>
            {
                new Office
                {
                    Id = 1,
                    OrganizationId = 2
                },
                new Office
                {
                    Id = 2,
                    OrganizationId = 2
                }
            }.AsQueryable();

            var books = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    Title = "asd",
                    Code = "123213",
                    OrganizationId = 2
                }
            }.AsQueryable();

            _booksDbSet.SetDbSetData(books);
            _officesDbSet.SetDbSetData(offices);
        }

        private void MockApplicationUsers()
        {
            var user1 = new ApplicationUser
            {
                Id = "testUser1",
                FirstName = "name1",
                LastName = "surname1"
            };

            var user2 = new ApplicationUser
            {
                Id = "testUser2",
                FirstName = "name2",
                LastName = "surname2"
            };

            var applicationUsers = new List<ApplicationUser>()
            {
                user1,
                user2
            };

            _userDbSet.SetDbSetData(applicationUsers);
        }

        private void MockEditBook()
        {
            var offices = new List<Office>
            {
                new Office
                {
                    Id = 1,
                    OrganizationId = 2
                },
                new Office
                {
                    Id = 2,
                    OrganizationId = 2
                }
            }.AsQueryable();

            var books = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    Title = "asd",
                    Code = "123213",
                    OrganizationId = 2,
                    BookOffices = new List<BookOffice>
                    {
                        new BookOffice
                        {
                            Id = 1,
                            OfficeId = 1,
                            OrganizationId = 2,
                            Quantity = 20,
                            BookId = 1
                        }
                    }
                }
            }.AsQueryable();

            _booksDbSet.SetDbSetData(books);
            _officesDbSet.SetDbSetData(offices);
        }

        private void MockDeleteBookEntities()
        {
            var book1 = new Book
            {
                Author = "test1",
                Id = 1,
                OrganizationId = 2,
                Title = "test1"
            };

            var bookLogs1 = new List<BookLog>
            {
                new BookLog
                {
                    OrganizationId = 2,
                    BookOfficeId = 1,
                    Id = 1,
                    Returned = null,
                },
                new BookLog
                {
                    OrganizationId = 2,
                    BookOfficeId = 2,
                    Returned = null,
                }
            };

            var bookOffice = new List<BookOffice>
            {
                new BookOffice
                {
                    Id = 1,
                    OrganizationId = 2,
                    BookLogs = new List<BookLog>(),
                    Book = book1,
                    BookId = 1,
                    Quantity = 2
                },
                new BookOffice
                {
                    Id = 2,
                    OrganizationId = 2,
                    BookLogs = bookLogs1,
                    Book = book1,
                    BookId = 1,
                    Quantity = 2
                }
            };

            _bookOfficesDbSet.SetDbSetData(bookOffice.AsQueryable());
        }

        private void MockGetBookDetails()
        {
            var user1 = new ApplicationUser
            {
                Id = "testUser1",
                FirstName = "name1",
                LastName = "surname1"
            };

            var user2 = new ApplicationUser
            {
                Id = "testUser2",
                FirstName = "name2",
                LastName = "surname2"
            };

            var office1 = new Office
            {
                Id = 1,
                Name = "Office1",
                OrganizationId = 2
            };

            var book1 = new Book
            {
                Author = "test1",
                Id = 1,
                OrganizationId = 2,
                Title = "test1"
            };

            var bookLogs1 = new List<BookLog>
            {
                new BookLog
                {
                    OrganizationId = 2,
                    ApplicationUserId = "testUser1",
                    BookOfficeId = 1,
                    Id = 1,
                    Returned = null,
                    ApplicationUser = user1
                },
                new BookLog
                {
                    OrganizationId = 2,
                    ApplicationUserId = "testUser2",
                    BookOfficeId = 2,
                    Returned = null,
                    ApplicationUser = user2
                }
            };

            var bookOffice = new List<BookOffice>
            {
                new BookOffice
                {
                    Id = 1,
                    OrganizationId = 2,
                    OfficeId = 1,
                    BookLogs = new List<BookLog>(),
                    Book = book1,
                    Office = office1,
                    BookId = 1,
                    Quantity = 2
                },
                new BookOffice
                {
                    Id = 2,
                    OrganizationId = 2,
                    OfficeId = 2,
                    BookLogs = bookLogs1,
                    Book = book1,
                    Office = office1,
                    BookId = 1,
                    Quantity = 2
                }
            };

            _bookOfficesDbSet.SetDbSetData(bookOffice.AsQueryable());
        }

        private void MockBookRetrieval()
        {
            _booksDbSet.SetDbSetData(new List<Book>().AsQueryable());
            _bookInfoService.FindBookByIsbnAsync("123").Returns(Task.FromResult(new ExternalBookInfo { Author = "test", Url = "test", Title = "asd" }));
        }

        private void MockBooksByOffice()
        {
            var book1 = new Book
            {
                Id = 1,
                Author = "Test1",
                OrganizationId = 2,
                Title = "Test1"
            };

            var book2 = new Book
            {
                Id = 2,
                Author = "Test2",
                OrganizationId = 2,
                Title = "Test2search"
            };

            var office1 = new Office
            {
                Id = 1,
                Name = "Test1",
                OrganizationId = 2
            };

            var office2 = new Office
            {
                Id = 2,
                Name = "Test2",
                OrganizationId = 2
            };

            var user = new ApplicationUser
            {
                FirstName = "Test1",
                LastName = "Test1",
                Id = "testUserId"
            };

            var booksOfficeList = new List<BookOffice>
            {
                new BookOffice
                {
                    BookId = 1,
                    Book = book1,
                    OfficeId = 1,
                    Office = office1,
                    BookLogs = new List<BookLog>
                    {
                        new BookLog
                        {
                            Id = 1,
                            Returned = null,
                            OrganizationId = 2,
                            ApplicationUserId = "testUserId",
                            ApplicationUser = user
                        },
                        new BookLog
                        {
                            Id = 2,
                            Returned = DateTime.UtcNow,
                            OrganizationId = 2,
                            ApplicationUserId = "testUserId",
                            ApplicationUser = user
                        }
                    },
                    Quantity = 2,
                    OrganizationId = 2
                },
                new BookOffice
                {
                    BookId = 2,
                    Book = book2,
                    OfficeId = 2,
                    Office = office2,
                    BookLogs = new List<BookLog>
                    {
                        new BookLog
                        {
                            Id = 3,
                            Returned = null,
                            OrganizationId = 2,
                            ApplicationUserId = "testUserId",
                            ApplicationUser = user
                        },
                        new BookLog
                        {
                            Id = 4,
                            Returned = DateTime.UtcNow,
                            OrganizationId = 2,
                            ApplicationUserId = "testUserId",
                            ApplicationUser = user
                        }
                    },
                    Quantity = 2,
                    OrganizationId = 2
                },
                new BookOffice
                {
                    BookId = 2,
                    Book = book2,
                    OfficeId = 1,
                    Office = office2,
                    BookLogs = new List<BookLog>(),
                    Quantity = 3,
                    OrganizationId = 2
                }
            };
            _bookOfficesDbSet.SetDbSetData(booksOfficeList.AsQueryable());
        }
        #endregion
    }
}
