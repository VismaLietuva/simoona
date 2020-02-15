using System;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.SystemClock
{
    public class SystemClock : ISystemClock
    {
        DateTime ISystemClock.UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
