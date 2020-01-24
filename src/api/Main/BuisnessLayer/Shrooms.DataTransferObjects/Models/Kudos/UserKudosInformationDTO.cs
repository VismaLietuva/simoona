using System;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class UserKudosInformationDTO
    {
        public KudosTypeDTO Type { get; set; }

        public string Comments { get; set; }

        public decimal Points { get; set; }

        public int MultiplyBy { get; set; }

        public ApplicationUserDTO Sender { get; set; }

        public DateTime Created { get; set; }
    }
}
