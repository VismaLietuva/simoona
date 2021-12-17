using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Books;
using Shrooms.Premium.DataTransferObjects.Models.Books.BookDetails;
using Shrooms.Premium.DataTransferObjects.Models.Books.BooksByOffice;
using Shrooms.Premium.DataTransferObjects.Models.LazyPaged;

namespace Shrooms.Premium.Domain.Services.Books
{
    public interface IBookService
    {
        Task AddBookAsync(NewBookDto bookDto);
        Task TakeBookAsync(BookTakeDto bookDto);
        Task EditBookAsync(EditBookDto editedBook);
        Task DeleteBookAsync(int bookOfficeId, UserAndOrganizationDto userOrg);
        Task TakeBookAsync(int bookOfficeId, UserAndOrganizationDto userAndOrg);
        Task ReturnBookAsync(int bookOfficeId, UserAndOrganizationDto userAndOrg);
        Task ReportBookAsync(BookReportDto bookReport, UserAndOrganizationDto userAndOrg);
        void UpdateBookCovers();
        Task<RetrievedBookInfoDto> FindBookByIsbnAsync(string isbn, int organizationId);
        Task<ILazyPaged<BooksByOfficeDto>> GetBooksByOfficeAsync(BooksByOfficeOptionsDto options);
        Task<BookDetailsDto> GetBookDetailsAsync(int bookOfficeId, UserAndOrganizationDto userOrg);
        Task<BookDetailsAdministrationDto> GetBookDetailsWithOfficesAsync(int bookOfficeId, UserAndOrganizationDto userOrg);
    }
}
