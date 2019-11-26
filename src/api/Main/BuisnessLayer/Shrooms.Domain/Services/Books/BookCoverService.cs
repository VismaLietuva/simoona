using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.GoogleBookService;

namespace Shrooms.Domain.Services.Books
{
    public class BookCoverService : IBackgroundWorker, IBookCoverService
    {
        private readonly IDbSet<Book> _booksDbSet;
        private readonly IUnitOfWork2 _uow;
        private readonly IBookInfoService _bookService;

        public BookCoverService(IUnitOfWork2 uow, IBookInfoService bookService)
        {
            _uow = uow;
            _bookService = bookService;
            _booksDbSet = _uow.GetDbSet<Book>();
        }


        public void UpdateBookCovers()
        {
            var booksWithoutCover = _booksDbSet.Where(book => book.BookCoverUrl == null);

            foreach (var book in booksWithoutCover)
            {
               var bookInfo = _bookService.FindBookByIsbnAsync(book.Code).Result;

               if (bookInfo?.CoverImageUrl != null)
               {
                    book.BookCoverUrl = bookInfo.CoverImageUrl;
               }
            }

            _uow.SaveChanges();
        }
    }
}