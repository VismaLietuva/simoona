using Shrooms.Contracts.Infrastructure.Email;

namespace Shrooms.Contracts.DataTransferObjects.Wall
{
    public class WallMemberEmailReceiverDto : IEmailReceiver
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string TimeZoneKey { get; set; }
    }
}
