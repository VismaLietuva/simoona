using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.EntityModels.Models.Multiwall
{
    public class PostWatcher
    {
        [Key, Column(Order = 0)]
        public int PostId { get; set; }
        [Key, Column(Order = 1)]
        public Guid UserId { get; set; }
    }
}
