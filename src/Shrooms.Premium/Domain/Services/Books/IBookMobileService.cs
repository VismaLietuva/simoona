using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Books;

namespace Shrooms.Premium.Domain.Services.Books
{
    public interface IBookMobileService
    {
        Task PostBookAsync(BookMobilePostDto bookDto);

        Task<IEnumerable<BookMobileLogDto>> ReturnBookAsync(BookMobileReturnDto bookDto);

        Task ReturnSpecificBookAsync(int bookLogId);

        Task<RetrievedBookInfoDto> GetBookAsync(BookMobileGetDto bookDto);

        Task<IEnumerable<MobileUserDto>> GetUsersForAutoCompleteAsync(string search, int organizationId);

        Task<IEnumerable<OfficeBookDto>> GetOfficesAsync(int organizationId);

        Task<RetrievedBookInfoDto> GetBookForPostAsync(string code, int organizationId);
    }
}
