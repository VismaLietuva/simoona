using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Books
{
    public interface IBookCoverService
    {
        Task UpdateBookCoversAsync();
    }
}