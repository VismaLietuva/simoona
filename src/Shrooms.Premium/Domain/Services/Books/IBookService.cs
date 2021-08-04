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
        Task AddBookAsync(NewBookDTO bookDto);
        Task TakeBookAsync(BookTakeDTO bookDTO);
        Task EditBookAsync(EditBookDTO editedBook);
        Task DeleteBookAsync(int bookOfficeId, UserAndOrganizationDTO userOrg);
        Task TakeBookAsync(int bookOfficeId, UserAndOrganizationDTO userAndOrg);
        Task ReturnBookAsync(int bookOfficeId, UserAndOrganizationDTO userAndOrg);
        Task ReportBookAsync(BookReportDTO bookReport, UserAndOrganizationDTO userAndOrg);
        void UpdateBookCovers();
        Task<RetrievedBookInfoDTO> FindBookByIsbnAsync(string isbn, int organizationId);
        Task<ILazyPaged<BooksByOfficeDTO>> GetBooksByOfficeAsync(BooksByOfficeOptionsDTO options);
        Task<BookDetailsDTO> GetBookDetailsAsync(int bookOfficeId, UserAndOrganizationDTO userOrg);
        Task<BookDetailsAdministrationDTO> GetBookDetailsWithOfficesAsync(int bookOfficeId, UserAndOrganizationDTO userOrg);
    }
}
