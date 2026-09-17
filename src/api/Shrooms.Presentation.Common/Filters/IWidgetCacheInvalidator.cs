using System.Threading;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Common.Filters
{
    /// <summary>
    /// Drops cached widget responses for the current organization after a write that changes them.
    /// </summary>
    public interface IWidgetCacheInvalidator
    {
        Task InvalidateAsync(string cacheGroup, CancellationToken cancellationToken = default);
    }
}
