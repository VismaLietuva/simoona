using System;
using System.Threading.Tasks;

namespace Shrooms.Contracts.Infrastructure
{
    public interface IAsyncRunner
    {
        void Run<T>(Func<T, Task> action, string tenantName);
    }
}