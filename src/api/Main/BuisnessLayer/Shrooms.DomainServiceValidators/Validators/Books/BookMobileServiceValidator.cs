using System.Collections.Generic;
using System.Linq;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.DomainExceptions.Exceptions.Book;
using Shrooms.EntityModels.Models;
using Resources = Shrooms.Resources.Models.Books;

namespace DomainServiceValidators.Validators.Books
{
    public class BookMobileServiceValidator : IBookMobileServiceValidator
    {
        public void ThrowIfBookExist(bool bookExists)
        {
            if (bookExists)
            {
                throw new BookException(Resources.Books.BookAlreadyExist);
            }
        }

        public void ThrowIfBookDoesNotExist(bool bookExists)
        {
            if (!bookExists)
            {
                throw new BookException(Resources.Books.NoBook);
            }
        }

        public void ThrowIfThereIsNoBookToReturn(bool bookExists)
        {
            if (!bookExists)
            {
                throw new BookException(Resources.Books.NoBooksToReturn);
            }
        }

        public void ChecksIfUserHasAlreadyBorrowedSameBook(IEnumerable<string> borrowedBookUserIds, string applicationUserId)
        {
            if (borrowedBookUserIds != null)
            {
                var bookAlreadyBorrowed = borrowedBookUserIds.Contains(applicationUserId);
                if (bookAlreadyBorrowed)
                {
                    throw new BookException(Resources.Books.UserAlreadyHasSameBook);
                }
            }
        }

        public void ThrowIfOfficeDoesNotExist(bool officeExists)
        {
            if (!officeExists)
            {
                throw new BookException(Resources.Books.NoOffice);
            }
        }

        public void ThrowIfBookIsAlreadyBorrowed(MobileBookOfficeLogsDTO officeBookWithLogs)
        {
            if (officeBookWithLogs.LogsUserIDs != null)
            {
                var availableBooks = officeBookWithLogs.Quantity - officeBookWithLogs.LogsUserIDs.Count();

                if (availableBooks < 1)
                {
                    throw new BookException(Resources.Books.NoAvailableBooks);
                }
            }
        }

        public void ThrowIfUserDoesNotExist(ApplicationUser applicationUser)
        {
            if (applicationUser == null)
            {
                throw new BookException(Resources.Books.UserDoesNotExist);
            }
        }

        public void ThrowIfBookDoesNotExistGoogleAPI(bool bookExists)
        {
            if (!bookExists)
            {
                throw new BookException(Resources.Books.NoBooksInGoogleApi);
            }
        }
    }
}
