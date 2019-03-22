using System;

namespace Shrooms.DataTransferObjects.Models.Committees
{
    public class CommitteeSuggestionViewDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }

        public ApplicationUserMinimalViewModelDto User { get; set; }
    }
}
