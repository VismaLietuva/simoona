using System.ComponentModel.DataAnnotations;
using Shrooms.Host.Contracts.Constants;

namespace Shrooms.WebViewModels.Models.Monitors
{
    public class CreateMonitorViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(WebApiConstants.MonitorNameMaxLength)]
        public string Name { get; set; }
    }
}
