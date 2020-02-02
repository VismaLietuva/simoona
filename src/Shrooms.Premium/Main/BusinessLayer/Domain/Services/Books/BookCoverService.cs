using System;
using System.Data.Entity;
using System.Linq;
using Shrooms.EntityModels.Models.Books;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.GoogleBookApiService;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Books
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