using AutoMapper;
using Shrooms.API.Filters;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.DomainExceptions.Exceptions.Book;
using Shrooms.WebViewModels.Models.Book;
using Shrooms.WebViewModels.Models.Book.BookDetails;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Shrooms.Domain.Services.Books;

namespace Shrooms.API.Controllers.Book
{
    [AllowAnonymous]
    [HMACAuthentication]
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
        public IHttpActionResult GetBook([FromUri] BookMobileGetViewModel bookViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDTO = _mapper.Map<BookMobileGetViewModel, BookMobileGetDTO>(bookViewModel);

            try
            {
                var getBookDTO = _bookMobileService.GetBook(bookDTO);
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
        public IHttpActionResult GetUsersForAutoComplete(string search, int organizationId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var mobileUsersDTO = _bookMobileService.GetUsersForAutoComplete(search, organizationId);
            var mobileUserViewModel = _mapper.Map<IEnumerable<MobileUserDTO>, IEnumerable<MobileUserViewModel>>(mobileUsersDTO);
            return Ok(mobileUserViewModel);
        }

        [HttpGet]
        public IHttpActionResult GetOffices(int organizationId)
        {
            var officeBookDTO = _bookMobileService.GetOffices(organizationId);
            var officeBookViewModel = _mapper.Map<IEnumerable<OfficeBookDTO>, IEnumerable<OfficeBookViewModel>>(officeBookDTO);
            return Ok(officeBookViewModel);
        }

        [HttpPost]
        public IHttpActionResult PostBook(BookMobilePostViewModel bookViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDTO = _mapper.Map<BookMobilePostViewModel, BookMobilePostDTO>(bookViewModel);

            try
            {
                _bookMobileService.PostBook(bookDTO);
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        [HttpPut]
        public IHttpActionResult ReturnBook(BookMobileReturnViewModel bookViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDTO = _mapper.Map<BookMobileReturnViewModel, BookMobileReturnDTO>(bookViewModel);

            try
            {
                var bookLogsDTO = _bookMobileService.ReturnBook(bookDTO);
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
        public IHttpActionResult ReturnSpecificBook(int bookLogId)
        {
            try
            {
                _bookMobileService.ReturnSpecificBook(bookLogId);
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpPut]
        public IHttpActionResult TakeBook(BookMobileTakeViewModel bookViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDTO = _mapper.Map<BookMobileTakeViewModel, BookTakeDTO>(bookViewModel);

            try
            {
                _bookService.TakeBook(bookDTO);
                return Ok();
            }
            catch (BookException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
