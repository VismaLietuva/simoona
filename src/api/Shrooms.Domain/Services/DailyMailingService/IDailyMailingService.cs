using System.Threading.Tasks;

namespace Shrooms.Domain.Services.DailyMailingService
{
    public interface IDailyMailingService
    {
        Task SendDigestedWallPostsAsync();
    }
}