using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.BusinessLayer;

namespace Shrooms.WebViewModels.Models.Kudos
{
    public class KudosRejectViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(ConstBusinessLayer.MaxMessageLength)]
        public string KudosRejectionMessage { get; set; }
    }
}
