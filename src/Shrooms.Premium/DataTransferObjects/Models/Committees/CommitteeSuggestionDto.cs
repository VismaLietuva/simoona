using System;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.Committees
{
    public class CommitteeSuggestionDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }

        public ApplicationUserMinimalDto User { get; set; }
    }
}
