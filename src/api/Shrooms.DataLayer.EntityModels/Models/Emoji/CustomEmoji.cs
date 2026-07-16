using System.ComponentModel.DataAnnotations;

namespace Shrooms.DataLayer.EntityModels.Models.Emoji
{
    public class CustomEmoji : SoftDeletableModelWithOrg
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public string BlobName { get; set; }

        [Required]
        public string AuthorId { get; set; }

        public ApplicationUser Author { get; set; }
    }
}
