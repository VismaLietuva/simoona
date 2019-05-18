using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.WebApi;

namespace Shrooms.WebViewModels.Models.Monitors
{
    public class CreateMonitorViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(WebApiConstants.MonitorNameMaxLength)]
        public string Name { get; set; }
    }
}
