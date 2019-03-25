using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.EntityValidationValues;

namespace Shrooms.WebViewModels.Models.Wall.Posts
{
    public class CreateWallPostViewModel
    {
        [Required]
        [StringLength(ValidationConstants.MaxPostMessageBodyLength)]
        public string MessageBody { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int WallId { get; set; }

        public string PictureId { get; set; }
    }
}
