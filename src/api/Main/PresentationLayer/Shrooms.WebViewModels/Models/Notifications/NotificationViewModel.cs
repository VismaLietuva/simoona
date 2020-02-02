using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models.Notifications
{
    public class NotificationViewModel
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public int id { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public string pictureId { get; set; }

        public SourcesViewModel sourceIds { get; set; }

        public int type { get; set; }

        public List<int> stackedIds { get; set; }

        public int? others { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
