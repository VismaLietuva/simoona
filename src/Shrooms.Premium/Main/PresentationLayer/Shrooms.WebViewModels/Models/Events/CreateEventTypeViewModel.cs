using Shrooms.Constants.WebApi;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.Events
{
    public class CreateEventTypeViewModel
    {
        public bool IsSingleJoin { get; set; }
        public bool IsFoodRelated { get; set; }

        [Required]
        [StringLength(ConstWebApi.EventTypeNameMaxLength)]
        public string Name { get; set; }
    }
}
