using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Wall
{
    public class UpdateWallDto : UserAndOrganizationDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Logo { get; set; }

        public IEnumerable<string> ModeratorsIds { get; set; }
    }
}
