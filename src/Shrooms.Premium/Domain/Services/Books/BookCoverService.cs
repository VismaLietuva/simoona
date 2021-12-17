using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.Premium.Infrastructure.GoogleBookApiService;

namespace Shrooms.Premium.Domain.Services.Books
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

        public async Task UpdateBookCoversAsync()
        {
            var booksWithoutCover = _booksDbSet.Where(book => book.BookCoverUrl == null);

            foreach (var book in booksWithoutCover)
            {
                try
                {
                    var bookInfo = await _bookService.FindBookByIsbnAsync(book.Code);

                    if (bookInfo?.CoverImageUrl == null)
                    {
                        continue;
                    }

                    book.BookCoverUrl = bookInfo.CoverImageUrl;
                    await _uow.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }
    }
}