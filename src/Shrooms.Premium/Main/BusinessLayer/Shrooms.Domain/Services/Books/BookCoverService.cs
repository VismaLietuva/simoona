using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.GoogleBookService;
using Shrooms.Infrastructure.Books;
using System;
using Shrooms.Infrastructure.Logger;

namespace Shrooms.Domain.Services.Books
{
    public class BookCoverService : IBackgroundWorker, IBookCoverService
    {
        private readonly IDbSet<Book> _booksDbSet;
        private readonly IUnitOfWork2 _uow;
        private readonly IBookInfoService _bookService;
        private readonly ILogger _logger;

        public BookCoverService(IUnitOfWork2 uow, IBookInfoService bookService, ILogger logger)
        {
            _uow = uow;
            _bookService = bookService;
            _booksDbSet = _uow.GetDbSet<Book>();
            _logger = logger;
        }


        public void UpdateBookCovers()
        {
            var booksWithoutCover = _booksDbSet.Where(book => book.BookCoverUrl == null);

            foreach (var book in booksWithoutCover)
            {
                try
                {
                    var bookInfo = _bookService.FindBookByIsbnAsync(book.Code).Result;

                    if (bookInfo != null)
                    {
                        if (bookInfo.CoverImageUrl != null)
                        {
                            book.BookCoverUrl = bookInfo.CoverImageUrl;
                        }

                        _uow.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

            }
        }
    }
}