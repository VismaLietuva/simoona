using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class UserKudosInformationDto
    {
        public KudosTypeDto Type { get; set; }

        public string Comments { get; set; }

        public decimal Points { get; set; }

        public int MultiplyBy { get; set; }

        public ApplicationUserDto Sender { get; set; }

        public DateTime Created { get; set; }
    }
}
