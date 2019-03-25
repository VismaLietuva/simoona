using System;
using System.Linq.Expressions;

namespace Shrooms.Infrastructure.FireAndForget
{
    public interface IJobScheduler
    {
        void Enqueue<T>(Expression<Action<T>> method);
    }
}