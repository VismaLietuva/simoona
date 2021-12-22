using System.Collections.Generic;
using System.Linq;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models;
using Shrooms.Premium.Domain.DomainExceptions.Book;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Books
{
    public class BookMobileServiceValidator : IBookMobileServiceValidator
    {
        public void ThrowIfBookExist(bool bookExists)
        {
            if (bookExists)
            {
                throw new BookException(Resources.Models.Books.Books.BookAlreadyExist);
            }
        }

        public void ThrowIfBookDoesNotExist(bool bookExists)
        {
            if (!bookExists)
            {
                throw new BookException(Resources.Models.Books.Books.NoBook);
            }
        }

        public void ThrowIfThereIsNoBookToReturn(bool bookExists)
        {
            if (!bookExists)
            {
                throw new BookException(Resources.Models.Books.Books.NoBooksToReturn);
            }
        }

        public void ChecksIfUserHasAlreadyBorrowedSameBook(IEnumerable<string> borrowedBookUserIds, string applicationUserId)
        {
            if (borrowedBookUserIds == null)
            {
                return;
            }

            var bookAlreadyBorrowed = borrowedBookUserIds.Contains(applicationUserId);
            if (bookAlreadyBorrowed)
            {
                throw new BookException(Resources.Models.Books.Books.UserAlreadyHasSameBook);
            }
        }

        public void ThrowIfOfficeDoesNotExist(bool officeExists)
        {
            if (!officeExists)
            {
                throw new BookException(Resources.Models.Books.Books.NoOffice);
            }
        }

        public void ThrowIfBookIsAlreadyBorrowed(MobileBookOfficeLogsDto officeBookWithLogs)
        {
            if (officeBookWithLogs.LogsUserIDs == null)
            {
                return;
            }

            var availableBooks = officeBookWithLogs.Quantity - officeBookWithLogs.LogsUserIDs.Count();

            if (availableBooks < 1)
            {
                throw new BookException(Resources.Models.Books.Books.NoAvailableBooks);
            }
        }

        public void ThrowIfUserDoesNotExist(ApplicationUser applicationUser)
        {
            if (applicationUser == null)
            {
                throw new BookException(Resources.Models.Books.Books.UserDoesNotExist);
            }
        }

        public void ThrowIfBookDoesNotExistGoogleApi(bool bookExists)
        {
            if (!bookExists)
            {
                throw new BookException(Resources.Models.Books.Books.NoBooksInGoogleApi);
            }
        }
    }
}
