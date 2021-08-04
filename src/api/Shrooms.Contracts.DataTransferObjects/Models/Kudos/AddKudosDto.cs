using Shrooms.Contracts.DataTransferObjects.Kudos;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class AddKudosDto
    {
        public AddKudosLogDto KudosLog { get; set; }

        public ApplicationUserDto ReceivingUser { get; set; }

        public ApplicationUserDto SendingUser { get; set; }

        public KudosTypeDto KudosType { get; set; }

        public decimal TotalKudosPointsInLog { get; set; }

        public decimal TotalPointsSent { get; set; }

        public string PictureId { get; set; }
    }
}
