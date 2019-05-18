using System;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events
{
    public class EventDetailsCommentViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string FullName { get; set; }

        public string Comment { get; set; }

        public DateTime Created { get; set; }

        public string ImageName { get; set; }
    }
}
