using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class FoodTeamEventViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Place { get; set; }

        public string ImageName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        // The options this user picked when joining; empty when the event has no options.
        public IEnumerable<string> SelectedOptions { get; set; } = new List<string>();
    }
}
