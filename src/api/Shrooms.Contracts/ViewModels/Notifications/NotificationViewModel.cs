using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.ViewModels.Notifications
{
    public class NotificationViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public SourcesViewModel SourceIds { get; set; }

        public int Type { get; set; }

        public List<int> StackedIds { get; set; }

        public int? Others { get; set; }

        public DateTime Created { get; set; }

        public bool IsRead { get; set; }
    }
}
