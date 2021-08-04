using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Books;

namespace Shrooms.Premium.Domain.Services.Books
{
    public interface IBookMobileService
    {
        Task PostBookAsync(BookMobilePostDTO bookDTO);

        Task<IEnumerable<BookMobileLogDTO>> ReturnBookAsync(BookMobileReturnDTO bookDTO);

        Task ReturnSpecificBookAsync(int bookLogId);

        Task<RetrievedBookInfoDTO> GetBookAsync(BookMobileGetDTO bookDTO);

        Task<IEnumerable<MobileUserDTO>> GetUsersForAutoCompleteAsync(string search, int organizationId);

        Task<IEnumerable<OfficeBookDTO>> GetOfficesAsync(int organizationId);

        Task<RetrievedBookInfoDTO> GetBookForPostAsync(string code, int organizationId);
    }
}
