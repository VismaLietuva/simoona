namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class UserKudosDTO
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public decimal TotalKudos { get; set; }

        public decimal RemainingKudos { get; set; }

        public decimal SpentKudos { get; set; }

        public string PictureId { get; set; }
    }
}
