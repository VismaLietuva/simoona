using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Models;
using System.Collections.Generic;
using System.Linq;

namespace Shrooms.Infrastructure.Email.Extensions
{
    public static class TimeZoneEmailGroupExtensions
    {
        public static IEnumerable<EmailDto> CreateEmails(this ITimeZoneEmailGroup group, TimeZoneGroup zoneGroup, string subject)
        {
            return zoneGroup.Values.Select(receivers => new EmailDto(receivers.Value, subject, group.Values[receivers.Key]));
        }
    }
}
