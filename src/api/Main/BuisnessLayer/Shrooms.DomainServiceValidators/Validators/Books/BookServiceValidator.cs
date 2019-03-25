using System;
using Shrooms.DomainExceptions.Exceptions.Book;
using static Shrooms.Constants.ErrorCodes.ErrorCodes;
using Resources = Shrooms.Resources.Models.Books;

namespace DomainServiceValidators.Validators.Books
{
    public class BookServiceValidator : IBookServiceValidator
    {
        public void CheckIfBookAlreadyExists(bool alreadyExists)
        {
            if (alreadyExists)
            {
                throw new BookException(BookAlreadyExistsCode);
            }
        }

        public void CheckIfBookAllQuantitiesAreNotZero(bool allQuantitiesNotZero)
        {
            if (allQuantitiesNotZero == false)
            {
                throw new BookException(BoolAllQuantitiesAreZeroCode);
            }
        }

        public void CheckIfRequestedOfficesExist(bool officeIsValid)
        {
            if (!officeIsValid)
            {
                throw new ArgumentException("Incorrect offices provided while creating new book");
            }
        }

        public void CheckIfBookOfficesFoundWhileDeleting(bool booksWereFound)
        {
            if (!booksWereFound)
            {
                throw new ArgumentException("Book was not found while deleting");
            }
        }

        public void CheckIfBookWasFoundByIsbnFromExternalProvider(object book)
        {
            if (book == null)
            {
                throw new BookException(BookNotFoundByExternalProviderCode);
            }
        }

        public void ThrowIfBookCannotBeReturned(bool bookExists)
        {
            if (!bookExists)
            {
                throw new BookException(Resources.Books.BookCannotBeReturned);
            }
        }
    }
}
