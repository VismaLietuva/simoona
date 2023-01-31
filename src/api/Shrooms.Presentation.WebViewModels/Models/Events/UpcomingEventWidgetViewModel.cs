using System;

namespace Shrooms.Presentation.WebViewModels.Models.Events
{
    public class UpcomingEventWidgetViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime RegistrationDeadlineDate { get; set; }

        public string TypeName { get; set; }
    }
}
