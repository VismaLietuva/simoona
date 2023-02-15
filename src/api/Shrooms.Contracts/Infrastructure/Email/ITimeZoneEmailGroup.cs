using System.Collections.Generic;

namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface ITimeZoneEmailGroup
    {
        Dictionary<string, string> Values { get; }
    }
}
