using Shrooms.Constants.BusinessLayer;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class NewKudosTypeDto : UserAndOrganizationDTO
    {
        public string Name { get; set; }

        public int Multiplier { get; set; }

        public BusinessLayerConstants.KudosTypeEnum Type { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}
