using System.Threading.Tasks;

namespace Shrooms.Domain.Services.SyncTokens
{
    public interface ISyncTokenService
    {
        Task<string> GetToken(string name);
        Task<string> UpdateAsync(string name, string syncToken);
        Task<string> CreateAsync(string name, string syncToken = "");
    }
}
