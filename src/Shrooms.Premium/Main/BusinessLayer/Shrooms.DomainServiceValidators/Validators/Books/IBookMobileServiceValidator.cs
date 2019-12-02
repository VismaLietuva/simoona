using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.EntityModels.Models;

namespace Shrooms.DomainServiceValidators.Validators.Books
{
    public interface IBookMobileServiceValidator
    {
        void ThrowIfBookExist(bool bookExists);
        void ThrowIfBookDoesNotExist(bool bookExists);
        void ThrowIfThereIsNoBookToReturn(bool bookExists);
        void ChecksIfUserHasAlreadyBorrowedSameBook(IEnumerable<string> borrowedBookUserIds, string applicationUserId);
        void ThrowIfOfficeDoesNotExist(bool officeExists);
        void ThrowIfBookIsAlreadyBorrowed(MobileBookOfficeLogsDTO officeBookWithLogs);
        void ThrowIfUserDoesNotExist(ApplicationUser applicationUser);
        void ThrowIfBookDoesNotExistGoogleAPI(bool bookExist);
    }
}
