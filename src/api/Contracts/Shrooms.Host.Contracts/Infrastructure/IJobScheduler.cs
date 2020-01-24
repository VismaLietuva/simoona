using System;
using System.Linq.Expressions;

namespace Shrooms.Host.Contracts.Infrastructure
{
    public interface IJobScheduler
    {
        void Enqueue<T>(Expression<Action<T>> method);
    }
}