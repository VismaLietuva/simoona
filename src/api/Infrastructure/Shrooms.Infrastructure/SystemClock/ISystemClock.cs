using System;

namespace Shrooms.Infrastructure.SystemClock
{
    public interface ISystemClock
    {
        DateTime UtcNow { get; }
    }
}
