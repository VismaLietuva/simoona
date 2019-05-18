using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.BusinessLayer;

namespace Shrooms.WebViewModels.Models.Users.Kudos
{
    public class KudosRejectViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(BusinessLayerConstants.MaxMessageLength)]
        public string KudosRejectionMessage { get; set; }
    }
}
