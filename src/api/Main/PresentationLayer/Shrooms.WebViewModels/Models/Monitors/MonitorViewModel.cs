using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.WebApi;

namespace Shrooms.WebViewModels.Models.Monitors
{
    public class MonitorViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(WebApiConstants.MonitorNameMaxLength)]
        public string Name { get; set; }
    }
}
