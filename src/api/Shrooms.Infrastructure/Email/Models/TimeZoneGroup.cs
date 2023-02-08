using System.Collections.Generic;

namespace Shrooms.Infrastructure.Email.Models
{
    public class TimeZoneGroup
    {
        public Dictionary<string, List<string>> Values { get; private set; }

        public TimeZoneGroup(Dictionary<string, List<string>> values)
        {
            Values = values;
        }

        public IEnumerable<string> GetTimeZoneKeys()
        {
            return Values.Keys;
        }
    }
}
