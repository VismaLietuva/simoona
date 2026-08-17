using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventOptionsViewModel
    {
        public int MaxOptions { get; set; }

        public IEnumerable<EventOptionViewModel> Options { get; set; }

        public IEnumerable<EventQuestionViewModel> Questions { get; set; } = new List<EventQuestionViewModel>();

        public IEnumerable<int> MyChosenOptions { get; set; } = new List<int>();
    }
}
