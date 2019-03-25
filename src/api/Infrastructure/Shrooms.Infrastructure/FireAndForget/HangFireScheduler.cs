using System;
using System.Linq.Expressions;
using Hangfire;

namespace Shrooms.Infrastructure.FireAndForget
{
    public class HangFireScheduler : IJobScheduler
    {
        public void Enqueue<T>(Expression<Action<T>> method)
        {
            BackgroundJob.Enqueue<T>(method);
        }
    }
}