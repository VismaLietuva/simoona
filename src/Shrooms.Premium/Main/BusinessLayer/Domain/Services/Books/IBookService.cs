using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.LazyPaged;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books.BookDetails;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books.BooksByOffice;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Books
{
    public interface IBookService
    {
        void AddBook(NewBookDTO bookDto);
        void TakeBook(BookTakeDTO bookDTO);
        void EditBook(EditBookDTO editedBook);
        void DeleteBook(int bookOfficeId, UserAndOrganizationDTO userOrg);
        void TakeBook(int bookOfficeId, UserAndOrganizationDTO userAndOrg);
        void ReturnBook(int bookOfficeId, UserAndOrganizationDTO userAndOrg);
        void ReportBook(BookReportDTO bookReport, UserAndOrganizationDTO userAndOrg);
        void UpdateBookCovers();
        Task<RetrievedBookInfoDTO> FindBookByIsbn(string isbn, int organizationId);
        ILazyPaged<BooksByOfficeDTO> GetBooksByOffice(BooksByOfficeOptionsDTO options);
        BookDetailsDTO GetBookDetails(int bookOfficeId, UserAndOrganizationDTO userOrg);
        BookDetailsAdministrationDTO GetBookDetailsWithOffices(int bookOfficeId, UserAndOrganizationDTO userOrg);
    }
}
