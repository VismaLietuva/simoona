using System;
using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Committees
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
