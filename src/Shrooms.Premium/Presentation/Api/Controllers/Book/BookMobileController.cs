using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.Books;
using Shrooms.Premium.Domain.DomainExceptions.Book;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Presentation.WebViewModels.Book;
using Shrooms.Premium.Presentation.WebViewModels.Book.BookDetails;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers.Book
{
    [AllowAnonymous]
    [HmacAuthentication]
    public class BookMobileController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly IBookService _bookService;
        private readonly IBookMobileService _bookMobileService;

        public BookMobileController(IMapper mapper, IBookMobileService bookMobileService, IBookService bookService)
        {
            _mapper = mapper;
            _bookMobileService = bookMobileService;
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetBook([FromUri] BookMobileGetViewModel bookViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDTO = _mapper.Map<BookMobileGetViewModel, BookMobileGetDTO>(bookViewModel);

            try
            {
                var getBookDTO = await _bookMobileService.GetBookAsync(bookDTO);
                var getBookViewModel = _mapper.Map<RetrievedBookInfoDTO, RetrievedMobileBookInfoViewModel>(getBookDTO);
                return Ok(getBookViewModel);
            }
            catch (BookException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetBookForPostAsync(string code, int organizationId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var getBookDTO = await _bookMobileService.GetBookForPostAsync(code, organizationId);
                var getBookViewModel = _mapper.Map<RetrievedBookInfoDTO, RetrievedBookForPostViewModel>(getBookDTO);
                return Ok(getBookViewModel);
            }
            catch (BookException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetUsersForAutoComplete(string search, int organizationId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var mobileUsersDTO = await _bookMobileService.GetUsersForAutoCompleteAsync(search, organizationId);
            var mobileUserViewModel = _mapper.Map<IEnumerable<MobileUserDTO>, IEnumerable<MobileUserViewModel>>(mobileUsersDTO);
            return Ok(mobileUserViewModel);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetOffices(int organizationId)
        {
            var officeBookDTO = await _bookMobileService.GetOfficesAsync(organizationId);
            var officeBookViewModel = _mapper.Map<IEnumerable<OfficeBookDTO>, IEnumerable<OfficeBookViewModel>>(officeBookDTO);
            return Ok(officeBookViewModel);
        }

        [HttpPost]
        public async Task<IHttpActionResult> PostBook(BookMobilePostViewModel bookViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDTO = _mapper.Map<BookMobilePostViewModel, BookMobilePostDTO>(bookViewModel);

            try
            {
                await _bookMobileService.PostBookAsync(bookDTO);
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        [HttpPut]
        public async Task<IHttpActionResult> ReturnBook(BookMobileReturnViewModel bookViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDTO = _mapper.Map<BookMobileReturnViewModel, BookMobileReturnDTO>(bookViewModel);

            try
            {
                var bookLogsDTO = await _bookMobileService.ReturnBookAsync(bookDTO);
                var bookLogsViewModel = bookLogsDTO == null
                    ? null
                    : _mapper.Map<IEnumerable<BookMobileLogDTO>, IEnumerable<BookMobileLogViewModel>>(bookLogsDTO);

                return Ok(bookLogsViewModel);
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<IHttpActionResult> ReturnSpecificBook(int bookLogId)
        {
            try
            {
                await _bookMobileService.ReturnSpecificBookAsync(bookLogId);
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpPut]
        public async Task<IHttpActionResult> TakeBook(BookMobileTakeViewModel bookViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDTO = _mapper.Map<BookMobileTakeViewModel, BookTakeDTO>(bookViewModel);

            try
            {
                await _bookService.TakeBookAsync(bookDTO);
                return Ok();
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
