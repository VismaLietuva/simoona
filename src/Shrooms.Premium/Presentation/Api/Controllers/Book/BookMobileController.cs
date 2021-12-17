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

            var bookDto = _mapper.Map<BookMobileGetViewModel, BookMobileGetDto>(bookViewModel);

            try
            {
                var getBookDto = await _bookMobileService.GetBookAsync(bookDto);
                var getBookViewModel = _mapper.Map<RetrievedBookInfoDto, RetrievedMobileBookInfoViewModel>(getBookDto);
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
                var getBookDto = await _bookMobileService.GetBookForPostAsync(code, organizationId);
                var getBookViewModel = _mapper.Map<RetrievedBookInfoDto, RetrievedBookForPostViewModel>(getBookDto);
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

            var mobileUsersDto = await _bookMobileService.GetUsersForAutoCompleteAsync(search, organizationId);
            var mobileUserViewModel = _mapper.Map<IEnumerable<MobileUserDto>, IEnumerable<MobileUserViewModel>>(mobileUsersDto);
            return Ok(mobileUserViewModel);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetOffices(int organizationId)
        {
            var officeBookDto = await _bookMobileService.GetOfficesAsync(organizationId);
            var officeBookViewModel = _mapper.Map<IEnumerable<OfficeBookDto>, IEnumerable<OfficeBookViewModel>>(officeBookDto);
            return Ok(officeBookViewModel);
        }

        [HttpPost]
        public async Task<IHttpActionResult> PostBook(BookMobilePostViewModel bookViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDto = _mapper.Map<BookMobilePostViewModel, BookMobilePostDto>(bookViewModel);

            try
            {
                await _bookMobileService.PostBookAsync(bookDto);
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

            var bookDto = _mapper.Map<BookMobileReturnViewModel, BookMobileReturnDto>(bookViewModel);

            try
            {
                var bookLogs = await _bookMobileService.ReturnBookAsync(bookDto);

                var bookLogsViewModel = bookLogs == null
                    ? null
                    : _mapper.Map<IEnumerable<BookMobileLogDto>, IEnumerable<BookMobileLogViewModel>>(bookLogs);

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

            var bookDto = _mapper.Map<BookMobileTakeViewModel, BookTakeDto>(bookViewModel);

            try
            {
                await _bookService.TakeBookAsync(bookDto);
                return Ok();
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
