using System.Collections.Concurrent;

namespace Shrooms.Infrastructure.CustomCache
{
    public class CustomCache<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, ICustomCache<TKey, TValue>
    {
        public bool TryRemoveEntry(TKey key)
        {
            TValue removedValue;
            return TryRemove(key, out removedValue);
        }
    }
}
