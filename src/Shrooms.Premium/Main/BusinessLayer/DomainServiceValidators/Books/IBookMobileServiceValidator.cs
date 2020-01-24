using System.Collections.Generic;
using Shrooms.EntityModels.Models;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.DomainServiceValidators.Books
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
