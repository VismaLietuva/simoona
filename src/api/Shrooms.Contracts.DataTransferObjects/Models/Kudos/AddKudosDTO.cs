namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class AddKudosDTO
    {
        public AddKudosLogDTO KudosLog { get; set; }

        public ApplicationUserDTO ReceivingUser { get; set; }

        public ApplicationUserDTO SendingUser { get; set; }

        public KudosTypeDTO KudosType { get; set; }

        public decimal TotalKudosPointsInLog { get; set; }

        public decimal TotalPointsSent { get; set; }

        public string PictureId { get; set; }
    }
}
