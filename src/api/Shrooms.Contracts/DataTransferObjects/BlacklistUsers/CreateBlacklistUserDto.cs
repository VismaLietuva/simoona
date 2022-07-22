using System;

namespace Shrooms.Contracts.DataTransferObjects.BlacklistUsers
{
    public class CreateBlacklistUserDto
    {
        public string UserId { get; set; }

        public DateTime EndDate { get; set; }

        public string Reason { get; set; }
    }
}
