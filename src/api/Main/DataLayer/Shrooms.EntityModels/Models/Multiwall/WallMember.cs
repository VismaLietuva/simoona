using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.EntityModels.Models.Multiwall
{
    public class WallMember : BaseModel
    {
        [ForeignKey("Wall")]
        public int WallId { get; set; }

        public Wall Wall { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public bool AppNotificationsEnabled { get; set; }

        public bool EmailNotificationsEnabled { get; set; }
    }
}
