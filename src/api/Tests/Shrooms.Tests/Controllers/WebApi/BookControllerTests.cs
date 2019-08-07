using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Results;
using NSubstitute;
using NUnit.Framework;
using Shrooms.API.Controllers.Book;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.LazyPaged;
using Shrooms.Domain.Services.Books;
using Shrooms.DomainExceptions.Exceptions.Book;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.UnitTests.ModelMappings;
using Shrooms.WebViewModels.Models.Book.BookDetails;
using Shrooms.WebViewModels.Models.Book.BooksByOffice;

namespace Shrooms.UnitTests.Controllers.WebApi
{
    [TestFixture]
    public class BookControllerTests
    {
        private BookController _bookController;
        private IBookService _bookService;

        [SetUp]
        public void TestInitializer()
        {
            _bookService = Substitute.For<IBookService>();
            var asyncRunner = Substitute.For<IAsyncRunner>();

            _bookController = new BookController(ModelMapper.Create(), _bookService, asyncRunner);
            _bookController.ControllerContext = Substitute.For<HttpControllerContext>();
            _bookController.Request = new HttpRequestMessage();
            _bookController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _bookController.Request.SetConfiguration(new HttpConfiguration());
            _bookController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim("OrganizationId", "1") }));
        }

        [Test]
        public void Book_AddBook_Should_Return_Invalid_Model_State()
        {
            var book = new NewBookViewModel
            {
                Isbn = "1"
            };
            _bookController.ModelState.AddModelError("test", "error");

            var response = _bookController.AddBook(book);

            Assert.IsInstanceOf<InvalidModelStateResult>(response);
        }

        [Test]
        public void Book_AddBook_Should_Return_Ok()
        {
            IEnumerable<NewBookQuantityViewModel> quantities = new List<NewBookQuantityViewModel>
            {
                new NewBookQuantityViewModel
                {
                    BookQuantity = 1,
                    OfficeId = 1
                }
            };

            var book = new NewBookViewModel
            {
                Isbn = "1",
                Title = "Testiness",
                Author = "Me the Great",
                QuantityByOffice = quantities
            };

            var response = _bookController.AddBook(book);

            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public void Book_DeleteBook_Should_Return_Bad_Request()
        {
            var id = 0;

            var response = _bookController.DeleteBook(id);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public void Book_DeleteBook_Should_Return_Ok()
        {
            var id = 1;

            var response = _bookController.DeleteBook(id);

            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public void Book_EditBook_Should_Return_Model_State_Invalid()
        {
            var book = new EditBookViewModel
            {
                Id = 1
            };
            _bookController.ModelState.AddModelError("test", "error");

            var response = _bookController.EditBook(book);

            Assert.IsInstanceOf<InvalidModelStateResult>(response);
        }

        [Test]
        public void Book_EditBook_Should_Return_Ok()
        {
            var book = new EditBookViewModel
            {
                Id = 1,
                Author = "MeMe",
                Isbn = "Mana",
                Title = "The Beast"
            };

            var response = _bookController.EditBook(book);

            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public void Book_GetBooksByOffice_Should_Return_Bad_Request()
        {
            var id = 0;

            var response = _bookController.GetBooksByOffice(id);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public void Book_GetBooksByOffice_Should_Return_Ok()
        {
            var id = 1;

            var response = _bookController.GetBooksByOffice(id);

            Assert.IsInstanceOf<OkNegotiatedContentResult<ILazyPaged<BooksByOfficeViewModel>>>(response);
        }

        [Test]
        public void Book_GetBookDetails_Should_Return_Bad_Request()
        {
            var id = 0;

            var response = _bookController.GetBookDetails(id);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public void Book_GetBookDetails_Should_Return_Ok()
        {
            var id = 1;

            var response = _bookController.GetBookDetails(id);

            Assert.IsInstanceOf<OkNegotiatedContentResult<BookDetailsViewModel>>(response);
        }

        [Test]
        public void Book_GetBookDetailsForAdministrator_Should_Return_Bad_Request()
        {
            var id = 0;

            var response = _bookController.GetBookDetailsForAdministrator(id);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public void Book_GetBookDetailsForAdministrator_Should_Return_Ok()
        {
            var id = 1;

            var response = _bookController.GetBookDetailsForAdministrator(id);

            Assert.IsInstanceOf<OkNegotiatedContentResult<BookDetailsAdministrationViewModel>>(response);
        }

        [Test]
        public void Book_ReturnBook_Should_Return_Bad_Request()
        {
            var id = 0;

            var response = _bookController.ReturnBook(id);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public void Book_ReturnBook_Should_Return_Ok()
        {
            var id = 1;

            var response = _bookController.ReturnBook(id);

            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public void Book_ReturnBook_Should_Return_Bad_Request_If_Excepiton()
        {
            var bookOfficeId = 1;
            var userAndOrg = new UserAndOrganizationDTO
            {
                UserId = "1",
                OrganizationId = 1
            };
            var exception = "test";

            _bookService.When(s => s.ReturnBook(bookOfficeId, userAndOrg)).Do(s => { throw new BookException(exception); });

            var response = _bookController.ReturnBook(bookOfficeId);

            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
        }

        [Test]
        public void Book_ReturnBookForAdmin_Should_Return_Bad_Request()
        {
            var id = 0;
            var userId = "id";

            var response = _bookController.ReturnBookForAdmin(id, userId);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public void Book_ReturnBookForAdmin_Should_Return_Ok()
        {
            var id = 1;
            var userId = "id";

            var response = _bookController.ReturnBookForAdmin(id, userId);

            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public void Book_ReturnBookForAdmin_Should_Return_Bad_Request_If_Exception()
        {
            var id = 1;
            var userId = "1";
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 1,
                UserId = "1"
            };
            var exception = "test";

            _bookService.When(s => s.ReturnBook(id, userAndOrg)).Do(s => { throw new BookException(exception); });

            var response = _bookController.ReturnBookForAdmin(id, userId);

            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
        }

        [Test]
        public void Book_TakeBook_Should_Return_Bad_Request()
        {
            var id = 0;

            var response = _bookController.TakeBook(id);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public void Book_TakeBook_Should_Return_Ok()
        {
            var id = 1;

            var response = _bookController.TakeBook(id);

            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public void Book_TakeBook_Should_Return_Bad_Request_If_Exception()
        {
            var id = 1;
            var exception = "test";
            var userAndOrg = new UserAndOrganizationDTO
            {
                UserId = "1",
                OrganizationId = 1
            };

            _bookService.When(s => s.TakeBook(id, userAndOrg)).Do(s => { throw new BookException(exception); });

            var response = _bookController.TakeBook(id);

            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
        }

        [Test]
        public async Task Book_FindByIsbn_Should_Return_Bad_Request()
        {
            string isbn = null;

            var response = await _bookController.FindByIsbn(isbn);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public async Task Book_FindByIsbn_Should_Return_Ok()
        {
            var isbn = "test";

            var response = await _bookController.FindByIsbn(isbn);

            Assert.IsInstanceOf<OkNegotiatedContentResult<RetrievedBookInfoViewModel>>(response);
        }

        [Test]
        public async Task Book_FindByIsbn_Should_Return_Bad_Request_If_Exception()
        {
            var isbn = "test";
            var exception = "test";
            var orgId = 1;

            _bookService.When(s => s.FindBookByIsbn(isbn, orgId)).Do(s => { throw new BookException(exception); });

            var response = await _bookController.FindByIsbn(isbn);

            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
        }
    }
}
