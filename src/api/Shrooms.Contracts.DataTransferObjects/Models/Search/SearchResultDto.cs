using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Search
{
    public class SearchResultDto
    {
        public IEnumerable<SearchResultGroupDto> Groups { get; set; }
    }

    public class SearchResultGroupDto
    {
        /// <summary>The section key the frontend uses: "posts", "people", "events", ...</summary>
        public string Kind { get; set; }

        public int Total { get; set; }

        public IEnumerable<SearchResultItemDto> Items { get; set; }
    }

    public class SearchResultItemDto
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? Date { get; set; }

        public string PictureId { get; set; }

        /// <summary>What the result links to when it is not the item itself, e.g. the kudos receiver.</summary>
        public string LinkId { get; set; }
    }
}
