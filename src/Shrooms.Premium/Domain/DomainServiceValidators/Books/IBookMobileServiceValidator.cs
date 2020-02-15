using System.Collections.Generic;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Books
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
        void ThrowIfBookDoesNotExistGoogleApi(bool bookExist);
    }
}
