using System;

namespace Shrooms.Contracts.Infrastructure
{
    public interface IAsyncRunner
    {
        void Run<T>(Action<T> action, string tenantName);
    }
}