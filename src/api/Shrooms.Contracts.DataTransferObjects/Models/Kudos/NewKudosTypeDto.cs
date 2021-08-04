using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class NewKudosTypeDto : UserAndOrganizationDto
    {
        public string Name { get; set; }

        public int Multiplier { get; set; }

        public KudosTypeEnum Type { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}
