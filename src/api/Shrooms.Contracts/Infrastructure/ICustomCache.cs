using System;

namespace Shrooms.Contracts.Infrastructure
{
    public interface ICustomCache<TKey, TValue>
    {
        bool TryAdd(TKey key, TValue value);
        bool TryRemoveEntry(TKey key);
        bool TryGetValue(TKey key, out TValue value);
        TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory);
        void Clear();
    }
}
