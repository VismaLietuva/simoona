using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventQueueViewModel
    {
        [Required]
        public Guid EventId { get; set; }

        public IEnumerable<int> ChosenOptions { get; set; }
    }
}
