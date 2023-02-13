using Shrooms.Contracts.Infrastructure.Email;

namespace Shrooms.Contracts.DataTransferObjects.Users
{
    public class UserEmailReceiverDto : IEmailReceiver
    {
        public string Email { get; set; }

        public string TimeZoneKey { get; set; }
    }
}
