using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.WebViewModels.Models.Users.Kudos
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
