using System;

namespace Shrooms.Contracts.Infrastructure
{
    public interface ISystemClock
    {
        DateTime UtcNow { get; }
    }
}
