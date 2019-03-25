using System;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class UserKudosInformationDTO
    {
        public KudosTypeDTO Type { get; set; }

        public string Comments { get; set; }

        public decimal Points { get; set; }

        public int MultiplyBy { get; set; }

        public ApplicationUser Sender { get; set; }

        public DateTime Created { get; set; }
    }
}
