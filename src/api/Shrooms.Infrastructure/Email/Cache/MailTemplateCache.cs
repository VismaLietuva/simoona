using Shrooms.Contracts.Infrastructure.Email;
using System;
using System.Collections.Concurrent;

namespace Shrooms.Infrastructure.Email.Cache
{
    public class MailTemplateCache : IMailTemplateCache
    {
        private readonly ConcurrentDictionary<Type, ICompiledEmailTemplate> _templateCache = new ConcurrentDictionary<Type, ICompiledEmailTemplate>();

        public void Add<T>(ICompiledEmailTemplate compiledEmailTemplate) where T : class
        {
            if (!_templateCache.TryAdd(typeof(T), compiledEmailTemplate))
            {
                throw new InvalidOperationException($"{typeof(T)} already exists in the cache");
            }
        }

        public ICompiledEmailTemplate Get<T>() where T : class
        {
            return _templateCache[typeof(T)];
        }
    }
}
