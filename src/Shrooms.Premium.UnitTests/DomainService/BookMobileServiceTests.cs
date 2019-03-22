using DomainServiceValidators.Validators.Books;
using NSubstitute;
using NUnit.Framework;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.DomainExceptions.Exceptions.Book;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Books;
using Shrooms.Infrastructure.GoogleBookService;
using Shrooms.UnitTests.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.Domain.Services.Books;

namespace Shrooms.UnitTests.DomainService
{
    [TestFixture]
    public class BookMobileServiceTests
    {
        private IBookMobileService _bookService;
        private IDbSet<ApplicationUser> _usersDbSet;
        private IDbSet<Office> _officesDbSet;
        private IDbSet<BookOffice> _bookOfficesDbSet;
        private IDbSet<Book> _booksDbSet;
        private IDbSet<BookLog> _bookLogsDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _usersDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            _usersDbSet.SetDbSetData(MockUsers());
            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _officesDbSet = Substitute.For<IDbSet<Office>>();
            _officesDbSet.SetDbSetData(MockOffices());
            uow.GetDbSet<Office>().Returns(_officesDbSet);

            _bookOfficesDbSet = Substitute.For<IDbSet<BookOffice>>();
            _bookOfficesDbSet.SetDbSetData(MockBookOffice());
            uow.GetDbSet<BookOffice>().Returns(_bookOfficesDbSet);

            _booksDbSet = Substitute.For<IDbSet<Book>>();
            _booksDbSet.SetDbSetData(MockBook());
            uow.GetDbSet<Book>().Returns(_booksDbSet);

            _bookLogsDbSet = Substitute.For<IDbSet<BookLog>>();
            _bookLogsDbSet.SetDbSetData(MockBookLog());
            uow.GetDbSet<BookLog>().Returns(_bookLogsDbSet);

            var validationService = new BookMobileServiceValidator();
            var bookInfoService = Substitute.For<IBookInfoService>();
            _bookService = new BookMobileService(uow, validationService, bookInfoService);
        }

        [Test]
        public void Should_Return_If_Gets_Wrong_Users_From_Autocomplete()
        {
            var result = _bookService.GetUsersForAutoComplete("Fir", 1);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("FirstName", result.First().FirstName);
        }

        [Test]
        public void Should_Return_If_Gets_Wrong_User_From_Autocomplete_By_Full_Name()
        {
            var result = _bookService.GetUsersForAutoComplete("Eglė Vąlkyščkytė", 1);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Vąlkyščkytė", result.First().LastName);
        }

        [Test]
        public void Should_Return_If_Gets_Wrong_User_From_Autocomplete_By_Username()
        {
            var result = _bookService.GetUsersForAutoComplete("user", 1);
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void Should_Return_If_Gets_Wrong_Users_From_Autocomplete_By_Similar_Surname()
        {
            var result = _bookService.GetUsersForAutoComplete("Surname", 1);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void Should_Return_If_Gets_Wrong_Number_Of_Offices()
        {
            var result = _bookService.GetOffices(1);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void Should_Return_If_Validation_For_Not_Existing_Book_Fails()
        {
            var bookMobileGetDTO = new BookMobileGetDTO()
            {
                Code = "0",
                OfficeId = 1,
                OrganizationId = 1
            };
            Assert.Throws<BookException>(() => _bookService.GetBook(bookMobileGetDTO));
        }

        [Test]
        public void Should_Return_If_Get_Book_Result_Has_Invalid_Data()
        {
            var bookMobileGetDTO = new BookMobileGetDTO()
            {
                Code = "1",
                OfficeId = 1,
                OrganizationId = 1
            };

            var result = _bookService.GetBook(bookMobileGetDTO);
            Assert.AreEqual("Author1", result.Author);
        }

        [Test]
        public void Should_Return_If_Get_Book_For_Post_Result_Has_Invalid_Data()
        {
            var result = _bookService.GetBookForPostAsync("1", 1);
            Assert.AreEqual("Author1", result.Result.Author);
        }

        [Test]
        public void Should_Return_If_Validation_For_Not_Existing_Book_In_Google_API_Fails()
        {
            Assert.Throws<BookException>(async () => await _bookService.GetBookForPostAsync("0", 1));
        }

        [Test]
        public void Should_Return_If_Post_New_Book_Does_Not_Add_New_Book()
        {
            var bookMobilePostDTO = new BookMobilePostDTO()
            {
                Code = "0",
                OrganizationId = 1,
            };

            _bookService.PostBook(bookMobilePostDTO);
            _booksDbSet.Received(1).Add(Arg.Any<Book>());
            _bookOfficesDbSet.Received(1).Add(Arg.Any<BookOffice>());
        }

        [Test]
        public void Should_Return_If_Post_New_Book_Does_Not_Add_Book_To_Another_Office()
        {
            var bookMobilePostDTO = new BookMobilePostDTO()
            {
                Code = "1",
                OrganizationId = 1,
                OfficeId = 2,
            };

            _bookService.PostBook(bookMobilePostDTO);
            _bookOfficesDbSet.Received(1).Add(Arg.Any<BookOffice>());
        }

        [Test]
        public void Should_Return_If_Post_New_Book_Create_Duplicate_In_Book_Office()
        {
            var bookMobilePostDTO = new BookMobilePostDTO()
            {
                Code = "1",
                OrganizationId = 1,
                OfficeId = 1,
            };

            Assert.Throws<BookException>(() => _bookService.PostBook(bookMobilePostDTO));
        }

        [Test]
        public void Should_Return_If_Adds_Book_To_Not_Existing_Office()
        {
            var bookMobilePostDTO = new BookMobilePostDTO()
            {
                Code = "1",
                OrganizationId = 1,
                OfficeId = 5,
            };

            Assert.Throws<BookException>(() => _bookService.PostBook(bookMobilePostDTO));
        }

        [Test]
        public void Should_Return_When_Not_Existing_Book_Is_Returned()
        {
            Assert.Throws<BookException>(() => _bookService.ReturnSpecificBook(100));
        }

        [Test]
        public void Should_Return_When_Same_Book_Log_Is_Returned_Second_Time()
        {
            Assert.Throws<BookException>(() => _bookService.ReturnSpecificBook(1));
        }

        [Test]
        public void Should_Return_When_Gets_Incorrect_List_Of_Already_Borrowed_Books()
        {
            var bookMobileReturnDTO = new BookMobileReturnDTO()
            {
                Code = "99",
                OrganizationId = 1,
                OfficeId = 1
            };

            var result = _bookService.ReturnBook(bookMobileReturnDTO);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(3, result.First().LogId);
        }

        #region dataMock
        private IQueryable<ApplicationUser> MockUsers()
        {
            return new List<ApplicationUser>()
            {
            new ApplicationUser()
            {
                FirstName = "FirstName",
                LastName = "LastName",
                UserName = "firstlas",
                Email = "firstname.lastname@visma.com",
                OrganizationId = 1
            },
            new ApplicationUser()
            {
                FirstName = "Name",
                LastName = "Surname",
                UserName = "name.surname@visma.com",
                Email = "name.surname@visma.com",
                OrganizationId = 1
            },
            new ApplicationUser()
            {
                FirstName = "Name2",
                LastName = "Surname2",
                UserName = "name2.surname2@visma.com",
                Email = "name2.surname2@visma.com",
                OrganizationId = 1
            },
            new ApplicationUser()
            {
                FirstName = "Test",
                LastName = "Again",
                UserName = "username",
                Email = "email",
                OrganizationId = 1
            },
            new ApplicationUser()
            {
                FirstName = "Eglė",
                LastName = "Vąlkyščkytė",
                UserName = "egle.valkysckyte@visma.com",
                Email = "egle.valkysckyte@visma.com",
                OrganizationId = 1
            },
            }.AsQueryable();
        }

        private IQueryable<Office> MockOffices()
        {
            return new List<Office>()
            {
                new Office()
                {
                    Name = "Vilnius",
                    Id = 1,
                    OrganizationId = 1
                },
                new Office()
                {
                    Name = "Kaunas",
                    Id = 2,
                    OrganizationId = 1
                },
                new Office()
                {
                    Name = "SmthElse",
                    Id = 3,
                    OrganizationId = 2
                }
            }.AsQueryable();
        }

        private IQueryable<BookOffice> MockBookOffice()
        {
            return new List<BookOffice>()
            {
                new BookOffice()
                {
                    Book = new Book()
                    {
                        Code = "1",
                        Author = "Author1",
                        Url = "Url1",
                        Title = "Title1",
                    },
                    Id = 1,
                    OfficeId = 1,
                    OrganizationId = 1
                },

                new BookOffice()
                {
                    Book = new Book()
                    {
                        Code = "2",
                        Author = "Author2",
                        Url = "Url2",
                        Title = "Title2",
                    },
                    Id = 2,
                    OfficeId = 1,
                    OrganizationId = 1
                },

                new BookOffice()
                {
                    Book = new Book()
                    {
                        Code = "3",
                        Author = "Author3",
                        Url = "Url3",
                        Title = "Title3",
                    },
                    Id = 3,
                    OfficeId = 1,
                    OrganizationId = 1
                },
            }.AsQueryable();
        }

        private IQueryable<Book> MockBook()
        {
            return new List<Book>()
            {
                    new Book()
                    {
                        Code = "1",
                        Author = "Author1",
                        Url = "Url1",
                        Title = "Title1",
                        OrganizationId = 1,
                        BookOffices = new List<BookOffice>()
                        {
                            new BookOffice()
                            {
                                OfficeId = 1
                            }
                        }
                    },

                    new Book()
                    {
                        Code = "2",
                        Author = "Author2",
                        Url = "Url2",
                        Title = "Title2",
                        OrganizationId = 1
                    },
                    new Book()
                    {
                        Code = "3",
                        Author = "Author3",
                        Url = "Url3",
                        Title = "Title3",
                        OrganizationId = 1
                    },
            }.AsQueryable();
        }

        private IQueryable<BookLog> MockBookLog()
        {
            return new List<BookLog>()
            {
                new BookLog()
                {
                    Id = 1,
                    Returned = DateTime.UtcNow,
                    OrganizationId = 1,
                    BookOffice = new BookOffice()
                    {
                        OfficeId = 1,
                        Book = new Book()
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser()
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                },

                new BookLog()
                {
                    Id = 2,
                    Returned = DateTime.UtcNow,
                    OrganizationId = 1,
                    BookOffice = new BookOffice()
                    {
                        OfficeId = 1,
                        Book = new Book()
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser()
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                },
                new BookLog()
                {
                    Id = 3,
                    Returned = null,
                    OrganizationId = 1,
                    BookOffice = new BookOffice()
                    {
                        OfficeId = 1,
                        Book = new Book()
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser()
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                },
                new BookLog()
                {
                    Id = 4,
                    Returned = null,
                    OrganizationId = 1,
                    BookOffice = new BookOffice()
                    {
                        OfficeId = 1,
                        Book = new Book()
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser()
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                },
                new BookLog()
                {
                    Id = 5,
                    Returned = null,
                    OrganizationId = 2,
                    BookOffice = new BookOffice()
                    {
                        OfficeId = 1,
                        Book = new Book()
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser()
                    {
                        FirstName = "user",
                        LastName = "user"
                    }
                },
                 new BookLog()
                {
                    Id = 6,
                    Returned = null,
                    OrganizationId = 1,
                    BookOffice = new BookOffice()
                    {
                        OfficeId = 2,
                        Book = new Book()
                        {
                            Code = "99"
                        }
                    },
                    ApplicationUser = new ApplicationUser()
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
