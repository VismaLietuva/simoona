using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Multiwall
{
    public class PostWatcher
    {
        [ForeignKey("Post")]
        public int PostId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual Post Post { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
