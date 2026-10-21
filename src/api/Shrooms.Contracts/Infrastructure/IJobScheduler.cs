using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Shrooms.Contracts.Infrastructure
{
    public interface IJobScheduler
    {
        void Enqueue<T>(Expression<Action<T>> method);

        string Schedule<T>(Expression<Func<T, Task>> method, DateTimeOffset enqueueAt);

        void Delete(string jobId);
    }
}