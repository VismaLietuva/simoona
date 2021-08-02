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
        void AddBook(NewBookDTO bookDto);
        Task TakeBookAsync(BookTakeDTO bookDTO);
        void EditBook(EditBookDTO editedBook);
        void DeleteBook(int bookOfficeId, UserAndOrganizationDTO userOrg);
        Task TakeBookAsync(int bookOfficeId, UserAndOrganizationDTO userAndOrg);
        void ReturnBook(int bookOfficeId, UserAndOrganizationDTO userAndOrg);
        Task ReportBookAsync(BookReportDTO bookReport, UserAndOrganizationDTO userAndOrg);
        void UpdateBookCovers();
        Task<RetrievedBookInfoDTO> FindBookByIsbn(string isbn, int organizationId);
        ILazyPaged<BooksByOfficeDTO> GetBooksByOffice(BooksByOfficeOptionsDTO options);
        BookDetailsDTO GetBookDetails(int bookOfficeId, UserAndOrganizationDTO userOrg);
        BookDetailsAdministrationDTO GetBookDetailsWithOffices(int bookOfficeId, UserAndOrganizationDTO userOrg);
    }
}
