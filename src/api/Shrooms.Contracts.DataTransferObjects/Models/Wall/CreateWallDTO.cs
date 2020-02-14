using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Models.Wall
{
    public class CreateWallDto : UserAndOrganizationDTO
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Logo { get; set; }

        public WallType Type { get; set; }

        public WallAccess Access { get; set; }

        public IEnumerable<string> MembersIds { get; set; }

        public IEnumerable<string> ModeratorsIds { get; set; }
    }
}
