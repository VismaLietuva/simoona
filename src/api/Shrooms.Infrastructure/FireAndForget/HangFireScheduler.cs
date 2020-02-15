using System;
using System.Linq.Expressions;
using Hangfire;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.FireAndForget
{
    public class HangFireScheduler : IJobScheduler
    {
        public void Enqueue<T>(Expression<Action<T>> method)
        {
            BackgroundJob.Enqueue(method);
        }
    }
}