using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Wall
{
    public class UpdateWallDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Logo { get; set; }

        public bool IsHidden { get; set; }

        public IEnumerable<string> ModeratorsIds { get; set; }
    }
}
