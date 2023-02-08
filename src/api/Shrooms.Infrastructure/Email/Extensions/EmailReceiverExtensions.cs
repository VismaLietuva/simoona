using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Models;
using System.Collections.Generic;
using System.Linq;

namespace Shrooms.Infrastructure.Email.Extensions
{
    public static class EmailReceiverExtensions
    {
        public static TimeZoneGroup CreateTimeZoneGroup(this IEnumerable<IEmailReceiver> receivers)
        {
            return new TimeZoneGroup(receivers.GroupBy(receiver => receiver.TimeZoneKey, receiver => receiver.Email)
                .ToDictionary(receiver => receiver.Key, receiver => receiver.ToList()));
        }
    }
}