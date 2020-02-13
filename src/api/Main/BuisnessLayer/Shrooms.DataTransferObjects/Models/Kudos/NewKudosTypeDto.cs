using Shrooms.Host.Contracts.Enums;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class NewKudosTypeDto : UserAndOrganizationDTO
    {
        public string Name { get; set; }

        public int Multiplier { get; set; }

        public KudosTypeEnum Type { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}
