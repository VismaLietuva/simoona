using System.Collections.Concurrent;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.CustomCache
{
    public class CustomCache<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, ICustomCache<TKey, TValue>
    {
        public bool TryRemoveEntry(TKey key)
        {
            return TryRemove(key, out _);
        }
    }
}
