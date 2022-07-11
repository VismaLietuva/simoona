using System;

namespace Shrooms.Contracts.DataTransferObjects.BlacklistStates
{
    public class BlacklistStateDto
    {
        public string UserId { get; set; }

        public DateTime EndDate { get; set; }

        public string Reason { get; set; }
    }
}
