using System;
using Shrooms.Presentation.WebViewModels.Models.User;

namespace Shrooms.Premium.Presentation.WebViewModels.Committees
{
    public class CommitteeSuggestionViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }

        public ApplicationUserMinimalViewModel User { get; set; }
    }
}