using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Books
{
    public interface IBookMobileService
    {
        void PostBook(BookMobilePostDTO bookDTO);

        IEnumerable<BookMobileLogDTO> ReturnBook(BookMobileReturnDTO bookDTO);

        void ReturnSpecificBook(int bookLogId);

        RetrievedBookInfoDTO GetBook(BookMobileGetDTO bookDTO);

        IEnumerable<MobileUserDTO> GetUsersForAutoComplete(string search, int organizationId);

        IEnumerable<OfficeBookDTO> GetOffices(int organizationId);

        Task<RetrievedBookInfoDTO> GetBookForPostAsync(string code, int organizationId);
    }
}
