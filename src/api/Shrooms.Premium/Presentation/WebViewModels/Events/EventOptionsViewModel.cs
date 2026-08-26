using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventOptionsViewModel
    {
        public int MaxOptions { get; set; }

        public IEnumerable<EventOptionViewModel> Options { get; set; }

        public IEnumerable<EventSignUpQuestionViewModel> Questions { get; set; } = new List<EventSignUpQuestionViewModel>();

        public IEnumerable<int> MyChosenOptions { get; set; } = new List<int>();
    }
}
