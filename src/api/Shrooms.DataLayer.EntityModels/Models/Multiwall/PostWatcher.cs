using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Multiwall
{
    public class PostWatcher
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Post")]
        public int PostId { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual Post Post { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
