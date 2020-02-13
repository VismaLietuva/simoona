using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models.Wall.Moderator;
using Shrooms.Host.Contracts.Enums;

namespace Shrooms.DataTransferObjects.Models.Wall
{
    public class WallDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public WallType Type { get; set; }

        public bool IsFollowing { get; set; }

        public string Description { get; set; }

        public IEnumerable<ModeratorDto> Moderators { get; set; }

        public string Logo { get; set; }

        public int TotalMembers { get; set; }

        public bool IsWallModerator { get; set; }
    }
}