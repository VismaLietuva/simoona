using Shrooms.WebViewModels.Models.User;
using System;

namespace Shrooms.WebViewModels.Models.Committees
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