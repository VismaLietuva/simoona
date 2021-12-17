using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.Premium.DataTransferObjects.Models.Books;
using Shrooms.Premium.Domain.DomainExceptions.Book;
using Shrooms.Premium.Domain.DomainServiceValidators.Books;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Infrastructure.GoogleBookApiService;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class BookMobileServiceTests
    {
        private IBookMobileService _bookService;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Office> _officesDbSet;
        private DbSet<BookOffice> _bookOfficesDbSet;
        private DbSet<Book> _booksDbSet;
        private DbSet<BookLog> _bookLogsDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IDbAsyncEnumerable<ApplicationUser>>();
            _usersDbSet.SetDbSetDataForAsync(MockUsers());
            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _officesDbSet = Substitute.For<DbSet<Office>, IQueryable<Office>, IDbAsyncEnumerable<Office>>();
            _officesDbSet.SetDbSetDataForAsync(MockOffices());
            uow.GetDbSet<Office>().Returns(_officesDbSet);

            _bookOfficesDbSet = Substitute.For<DbSet<BookOffice>, IQueryable<BookOffice>, IDbAsyncEnumerable<BookOffice>>();
            _bookOfficesDbSet.SetDbSetDataForAsync(MockBookOffice());
            uow.GetDbSet<BookOffice>().Returns(_bookOfficesDbSet);

            _booksDbSet = Substitute.For<DbSet<Book>, IQueryable<Book>, IDbAsyncEnumerable<Book>>();
            _booksDbSet.SetDbSetDataForAsync(MockBook());
            uow.GetDbSet<Book>().Returns(_booksDbSet);

            _bookLogsDbSet = Substitute.For<DbSet<BookLog>, IQueryable<BookLog>, IDbAsyncEnumerable<BookLog>>();
            _bookLogsDbSet.SetDbSetDataForAsync(MockBookLog());
            uow.GetDbSet<BookLog>().Returns(_bookLogsDbSet);

            var validationService = new BookMobileServiceValidator();
            var bookInfoService = Substitute.For<IBookInfoService>();
            _bookService = new BookMobileService(uow, validationService, bookInfoService);
        }

        [Test]
        public async Task Should_Return_If_Gets_Wrong_Users_From_Autocomplete()
        {
            var result = (await _bookService.GetUsersForAutoCompleteAsync("Fir", 1)).ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("FirstName", result.First().FirstName);
        }

        [Test]
        public async Task Should_Return_If_Gets_Wrong_User_From_Autocomplete_By_Full_Name()
        {
            var result = (await _bookService.GetUsersForAutoCompleteAsync("Eglė Eglė", 1)).ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Eglė", result.First().LastName);
        }

        [Test]
        public async Task Should_Return_If_Gets_Wrong_User_From_Autocomplete_By_Username()
        {
            var result = await _bookService.GetUsersForAutoCompleteAsync("user", 1);
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public async Task Should_Return_If_Gets_Wrong_Users_From_Autocomplete_By_Similar_Surname()
        {
            var result = await _bookService.GetUsersForAutoCompleteAsync("Surname", 1);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task Should_Return_If_Gets_Wrong_Number_Of_Offices()
        {
            var result = await _bookService.GetOfficesAsync(1);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void Should_Return_If_Validation_For_Not_Existing_Book_Fails()
        {
            var bookMobileGetDto = new BookMobileGetDto
            {
                Code = "0",
                OfficeId = 1,
                OrganizationId = 1
            };

            Assert.ThrowsAsync<BookException>(async () => await _bookService.GetBookAsync(bookMobileGetDto));
        }

        [Test]
        public async Task Should_Return_If_Get_Book_Result_Has_Invalid_Data()
        {
            var bookMobileGetDto = new BookMobileGetDto
            {
                Code = "1",
                OfficeId = 1,
                OrganizationId = 1
            };

            var result = await _bookService.GetBookAsync(bookMobileGetDto);
            Assert.AreEqual("Author1", result.Author);
        }

        [Test]
        public async Task Should_Return_If_Get_Book_For_Post_Result_Has_Invalid_Data()
        {
            var result = await _bookService.GetBookForPostAsync("1", 1);
            Assert.AreEqual("Author1", result.Author);
        }

        [Test]
        public void Should_Return_If_Validation_For_Not_Existing_Book_In_Google_API_Fails()
        {
            Assert.ThrowsAsync<BookException>(async () => await _bookService.GetBookForPostAsync("0", 1));
        }

        [Test]
        public async Task Should_Return_If_Post_New_Book_Does_Not_Add_New_Book()
        {
            var bookMobilePostDto = new BookMobilePostDto
            {
                Code = "0",
                OrganizationId = 1
            };

            await _bookService.PostBookAsync(bookMobilePostDto);
            _booksDbSet.Received(1).Add(Arg.Any<Book>());
            _bookOfficesDbSet.Received(1).Add(Arg.Any<BookOffice>());
        }

        [Test]
        public async Task Should_Return_If_Post_New_Book_Does_Not_Add_Book_To_Another_Office()
        {
            var bookMobilePostDto = new BookMobilePostDto
            {
                Code = "1",
                OrganizationId = 1,
                OfficeId = 2
            };

            await _bookService.PostBookAsync(bookMobilePostDto);
            _bookOfficesDbSet.Received(1).Add(Arg.Any<BookOffice>());
        }

        [Test]
        public void Should_Return_If_Post_New_Book_Create_Duplicate_In_Book_Office()
        {
            var bookMobilePostDto = new BookMobilePostDto
            {
                Code = "1",
                OrganizationId = 1,
                OfficeId = 1
            };

            Assert.ThrowsAsync<BookException>(async () => await _bookService.PostBookAsync(bookMobilePostDto));
        }

        [Test]
        public void Should_Return_If_Adds_Book_To_Not_Existing_Office()
        {
            var bookMobilePostDto = new BookMobilePostDto
            {
                Code = "1",
                OrganizationId = 1,
                OfficeId = 5
            };

            Assert.ThrowsAsync<BookException>(async () => await _bookService.PostBookAsync(bookMobilePostDto));
        }

        [Test]
        public void Should_Return_When_Not_Existing_Book_Is_Returned()
        {
            Assert.ThrowsAsync<BookException>(async () => await _bookService.ReturnSpecificBookAsync(100));
        }

        [Test]
        public void Should_Return_When_Same_Book_Log_Is_Returned_Second_Time()
        {
            Assert.ThrowsAsync<BookException>(async () => await _bookService.ReturnSpecificBookAsync(1));
        }

        [Test]
        public async Task Should_Return_When_Gets_Incorrect_List_Of_Already_Borrowed_Books()
        {
            var bookMobileReturnDto = new BookMobileReturnDto
            {
                Code = "99",
                OrganizationId = 1,
                OfficeId = 1
            };

            var result = (await _bookService.ReturnBookAsync(bookMobileReturnDto)).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(3, result.First().LogId);
        }

        #region dataMock
        private IQueryable<ApplicationUser> MockUsers()
        {
            return new List<ApplicationUser>
            {
            new ApplicationUser
            {
                FirstName = "FirstName",
                LastName = "LastName",
                UserName = "firstlas",
                Email = "firstname.lastname@visma.com",
                OrganizationId = 1
            },
            new ApplicationUser
            {
                FirstName = "Name",
                LastName = "Surname",
                UserName = "name.surname@visma.com",
                Email = "name.surname@visma.com",
                OrganizationId = 1
            },
            new ApplicationUser
            {
                FirstName = "Name2",
                LastName = "Surname2",
                UserName = "name2.surname2@visma.com",
                Email = "name2.surname2@visma.com",
                OrganizationId = 1
            },
            new ApplicationUser
            {
                FirstName = "Test",
                LastName = "Again",
                UserName = "username",
                Email = "email",
                OrganizationId = 1
            },
            new ApplicationUser
            {
                FirstName = "Eglė",
                LastName = "Eglė",
                UserName = "egle.valkysckyte@visma.com",
                Email = "egle.valkysckyte@visma.com",
                OrganizationId = 1
            }
            }.AsQueryable();
        }

        private IQueryable<Office> MockOffices()
        {
            return new List<Office>
            {
                new Office
                {
                    Name = "Vilnius",
                    Id = 1,
                    OrganizationId = 1
                },
                new Office
                {
                    Name = "Kaunas",
                    Id = 2,
                    OrganizationId = 1
                },
                new Office
                {
                    Name = "SmthElse",
                    Id = 3,
                    OrganizationId = 2
                }
            }.AsQueryable();
        }

        private IQueryable<BookOffice> MockBookOffice()
        {
            return new List<BookOffice>
            {
                new BookOffice
                {
                    Book = new Book
                    {
                        Code = "1",
                        Author = "Author1",
                        Url = "Url1",
                        Title = "Title1"
                    },
                    Id = 1,
                    OfficeId = 1,
                    OrganizationId = 1
                },

                new BookOffice
                {
                    Book = new Book
                    {
                        Code = "2",
                        Author = "Author2",
                        Url = "Url2",
                        Title = "Title2"
                    },
                    Id = 2,
                    OfficeId = 1,
                    OrganizationId = 1
                },

                new BookOffice
                {
                    Book = new Book
                    {
                        Code = "3",
                        Author = "Author3",
                        Url = "Url3",
                        Title = "Title3"
                    },
                    Id = 3,
                    OfficeId = 1,
                    OrganizationId = 1
                }
            }.AsQueryable();
        }

        private IQueryable<Book> MockBook()
        {
            return new List<Book>
            {
                    new Book
                    {
                        Code = "1",
                        Author = "Author1",
                        Url = "Url1",
                        Title = "Title1",
                        OrganizationId = 1,
                        BookOffices = new List<BookOffice>
                        {
                            new BookOffice
                            {
                                OfficeId = 1
                            }
                        }
                    },

                    new Book
                    {
                        Code = "2",
                        Author = "Author2",
                        Url = "Url2",
                        Title = "Title2",
                        OrganizationId = 1
                    },
                    new Book
                    {
                        Code = "3",
                        Author = "Author3",
                        Url = "Url3",
                        Title = "Title3",
                        OrganizationId = 1
                    }
            }.AsQueryable();
        }

        private IQueryable<BookLog> MockBookLog()
        {
            return new List<BookLog>
            {
                new BookLog
                {
                    Id = 1,
                    Returned = DateTime.UtcNow,
                    OrganizationId = 1,
                    BookOffice = new BookOffice
                    {
                        OfficeId = 1,
                        Book = new Book
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                },

                new BookLog
                {
                    Id = 2,
                    Returned = DateTime.UtcNow,
                    OrganizationId = 1,
                    BookOffice = new BookOffice
                    {
                        OfficeId = 1,
                        Book = new Book
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                },
                new BookLog
                {
                    Id = 3,
                    Returned = null,
                    OrganizationId = 1,
                    BookOffice = new BookOffice
                    {
                        OfficeId = 1,
                        Book = new Book
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                },
                new BookLog
                {
                    Id = 4,
                    Returned = null,
                    OrganizationId = 1,
                    BookOffice = new BookOffice
                    {
                        OfficeId = 1,
                        Book = new Book
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                },
                new BookLog
                {
                    Id = 5,
                    Returned = null,
                    OrganizationId = 2,
                    BookOffice = new BookOffice
                    {
                        OfficeId = 1,
                        Book = new Book
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                },
                new BookLog
                {
                    Id = 6,
                    Returned = null,
                    OrganizationId = 1,
                    BookOffice = new BookOffice
                    {
                        OfficeId = 2,
                        Book = new Book
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                }
            }.AsQueryable();
        }
        #endregion
    }
}
