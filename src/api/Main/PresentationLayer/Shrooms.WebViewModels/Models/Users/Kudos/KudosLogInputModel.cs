using System.ComponentModel.DataAnnotations;
using Shrooms.EntityModels.Models.Kudos;

namespace Shrooms.WebViewModels.Models.Kudos
{
    public class KudosLogInputModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public KudosType PointsType { get; set; }

        [Required]
        [Range(1, 10000)]
        public int MultipleBy { get; set; }

        [Required]
        [MaxLength(500)]
        public string Comment { get; set; }

        public string UserPassword { get; set; }
    }
}