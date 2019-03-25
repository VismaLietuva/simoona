using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class AddKudosDTO
    {
        public AddKudosLogDTO KudosLog { get; set; }

        public ApplicationUser ReceivingUser { get; set; }

        public ApplicationUser SendingUser { get; set; }

        public KudosType KudosType { get; set; }

        public decimal TotalKudosPointsInLog { get; set; }

        public decimal TotalPointsSent { get; set; }

        public string PictureId { get; set; }
    }
}
