using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.WebViewModels.Models.Monitors
{
    public class CreateMonitorViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(WebApiConstants.MonitorNameMaxLength)]
        public string Name { get; set; }
    }
}
