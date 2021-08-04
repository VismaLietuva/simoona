using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class KudosTypeDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Value { get; set; }

        public bool Hidden { get; set; }

        public bool IsNecessary { get; set; }

        public KudosTypeEnum Type { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}
