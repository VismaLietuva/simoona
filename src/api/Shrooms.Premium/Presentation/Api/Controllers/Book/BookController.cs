using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Books;
using Shrooms.Premium.DataTransferObjects.Models.Books.BookDetails;
using Shrooms.Premium.DataTransferObjects.Models.Books.BooksByOffice;
using Shrooms.Premium.DataTransferObjects.Models.LazyPaged;
using Shrooms.Premium.Domain.DomainExceptions.Book;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Presentation.WebViewModels.Book;
using Shrooms.Premium.Presentation.WebViewModels.Book.BookDetails;
using Shrooms.Premium.Presentation.WebViewModels.Book.BooksByOffice;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.Common.Helpers;

namespace Shrooms.Premium.Presentation.Api.Controllers.Book
{
    [Authorize]
    [Route("Book")]
    public class BookController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IBookService _bookService;

        public BookController(IMapper mapper, IBookService bookService)
        {
            _mapper = mapper;
            _bookService = bookService;
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Book)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddBook(NewBookViewModel book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDto = _mapper.Map<NewBookViewModel, NewBookDto>(book);
            SetOrganizationAndUser(bookDto);
            try
            {
                await _bookService.AddBookAsync(bookDto);
                return Ok();
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Book)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            if (bookId < 1)
            {
                return BadRequest();
            }

            var userOrg = GetUserAndOrganization();
            try
            {
                await _bookService.DeleteBookAsync(bookId, userOrg);
                return Ok();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpPut]
        [Route("Edit")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Book)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EditBook(EditBookViewModel book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDto = _mapper.Map<EditBookViewModel, EditBookDto>(book);
            SetOrganizationAndUser(bookDto);

            try
            {
                await _bookService.EditBookAsync(bookDto);
                return Ok();
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("ListByOffice")]
        [PermissionAuthorize(Permission = BasicPermissions.Book)]
        [ProducesResponseType(typeof(LazyPaged<BooksByOfficeViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBooksByOffice(int officeId, int page = 1, string searchString = null)
        {
            if (!string.IsNullOrEmpty(searchString) && searchString.Length < BusinessLayerConstants.MinCharactersInBookSearch || officeId < 1)
            {
                return BadRequest();
            }

            var options = new BooksByOfficeOptionsDto
            {
                OfficeId = officeId,
                Page = page,
                SearchString = searchString
            };

            SetOrganizationAndUser(options);

            var books = await _bookService.GetBooksByOfficeAsync(options);
            var result = _mapper.Map<ILazyPaged<BooksByOfficeDto>, LazyPaged<BooksByOfficeViewModel>>(books);
            return Ok(result);
        }

        [HttpGet]
        [Route("Details")]
        [PermissionAuthorize(Permission = BasicPermissions.Book)]
        [ProducesResponseType(typeof(BookDetailsAdministrationViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookDetails(int bookOfficeId)
        {
            if (bookOfficeId < 1)
            {
                return BadRequest();
            }

            var bookWithLogsDto = await _bookService.GetBookDetailsWithOfficesAsync(bookOfficeId, GetUserAndOrganization());
            var result = _mapper.Map<BookDetailsAdministrationDto, BookDetailsAdministrationViewModel>(bookWithLogsDto);
            return Ok(result);
        }

        [HttpPut]
        [Route("Return")]
        [PermissionAuthorize(Permission = BasicPermissions.Book)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReturnBook(int bookOfficeId)
        {
            if (bookOfficeId < 1)
            {
                return BadRequest();
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                await _bookService.ReturnBookAsync(bookOfficeId, userAndOrg);
                return Ok();
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route("Report")]
        [PermissionAuthorize(Permission = BasicPermissions.Book)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReportMissingBook(BookReportViewModel bookReport)
        {
            if (bookReport.BookOfficeId < 1)
            {
                return BadRequest();
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                var bookReportDto = _mapper.Map<BookReportViewModel, BookReportDto>(bookReport);
                await _bookService.ReportBookAsync(bookReportDto, userAndOrg);
                return Ok();
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route(template: "ReturnForAdmin")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Book)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReturnBookForAdmin(int bookOfficeId, string userId)
        {
            if (bookOfficeId < 1)
            {
                return BadRequest();
            }

            try
            {
                await _bookService.ReturnBookAsync(bookOfficeId: bookOfficeId, userAndOrg: new UserAndOrganizationDto
                {
                    OrganizationId = User.Identity.GetOrganizationId(),
                    UserId = userId
                });
                return Ok();
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route("Take")]
        [PermissionAuthorize(Permission = BasicPermissions.Book)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TakeBook(int bookOfficeId)
        {
            if (bookOfficeId < 1)
            {
                return BadRequest();
            }

            var userAndOrg = GetUserAndOrganization();

            try
            {
                await _bookService.TakeBookAsync(bookOfficeId, userAndOrg);
                return Ok();
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch]
        [Route("Covers")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Book)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult UpdateBookCovers()
        {
            _bookService.UpdateBookCovers();
            return Ok();
        }

        [HttpGet]
        [Route("FindByIsbn")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Book)]
        [ProducesResponseType(typeof(RetrievedBookInfoViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindByIsbn(string isbn)
        {
            if (string.IsNullOrEmpty(isbn))
            {
                return BadRequest();
            }

            try
            {
                var bookInfoDto = await _bookService.FindBookByIsbnAsync(isbn, GetUserAndOrganization().OrganizationId);
                var result = _mapper.Map<RetrievedBookInfoDto, RetrievedBookInfoViewModel>(bookInfoDto);
                return Ok(result);
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
