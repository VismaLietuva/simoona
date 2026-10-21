using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.FireAndForget
{
    public class HangFireScheduler : IJobScheduler
    {
        private readonly IBackgroundJobClient _client;

        public HangFireScheduler(IBackgroundJobClient client)
        {
            _client = client;
        }

        public void Enqueue<T>(Expression<Action<T>> method)
        {
            _client.Enqueue(method);
        }

        public string Schedule<T>(Expression<Func<T, Task>> method, DateTimeOffset enqueueAt)
        {
            return _client.Schedule(method, enqueueAt);
        }

        public void Delete(string jobId)
        {
            if (!string.IsNullOrEmpty(jobId))
            {
                _client.Delete(jobId);
            }
        }
    }
}