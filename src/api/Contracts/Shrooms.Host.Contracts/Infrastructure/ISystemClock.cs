using System;

namespace Shrooms.Host.Contracts.Infrastructure
{
    public interface ISystemClock
    {
        DateTime UtcNow { get; }
    }
}
