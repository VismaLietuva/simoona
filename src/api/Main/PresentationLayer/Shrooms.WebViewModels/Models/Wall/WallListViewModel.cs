using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.WebViewModels.Models.Wall.Moderator;

namespace Shrooms.WebViewModels.Models.Wall
{
    public class WallListViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsFollowing { get; set; }

        public string Description { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WallType Type { get; set; }

        public IEnumerable<ModeratorViewModel> Moderators { get; set; }

        public string Logo { get; set; }

        public int TotalMembers { get; set; }

        public bool IsWallModerator { get; set; }
    }
}