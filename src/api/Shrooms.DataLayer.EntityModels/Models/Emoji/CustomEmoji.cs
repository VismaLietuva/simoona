using System.ComponentModel.DataAnnotations;

namespace Shrooms.DataLayer.EntityModels.Models.Emoji
{
    public class CustomEmoji : BaseModelWithOrg
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public string BlobName { get; set; }
    }
}
