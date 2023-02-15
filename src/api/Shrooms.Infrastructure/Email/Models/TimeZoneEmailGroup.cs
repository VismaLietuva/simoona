using Shrooms.Contracts.Infrastructure.Email;
using System.Collections.Generic;

namespace Shrooms.Infrastructure.Email.Models
{
    public class TimeZoneEmailGroup : ITimeZoneEmailGroup
    {
        public Dictionary<string, string> Values { get; private set; }

        public TimeZoneEmailGroup(Dictionary<string, string> values)
        {
            Values = values;
        }
    }
}
