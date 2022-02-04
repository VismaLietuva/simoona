using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Multiwall
{
    public class Wall : BaseModelWithOrg
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Logo { get; set; }

        public bool IsHidden { get; set; }

        public WallType Type { get; set; }

        public WallAccess Access { get; set; }

        public virtual ICollection<Post> Posts { get; set; }

        public virtual ICollection<WallMember> Members { get; set; }

        public virtual ICollection<WallModerator> Moderators { get; set; }
    }
}
