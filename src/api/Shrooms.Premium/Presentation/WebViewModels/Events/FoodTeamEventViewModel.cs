using System;

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

        // The option this user picked when joining; null when the event has no options.
        // Food team events let a participant pick a single option, so this is not a collection.
        public string SelectedOption { get; set; }
    }
}
