using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Search
{
    public class GlobalSearchResultsDto
    {
        public IEnumerable<GlobalSearchPersonDto> People { get; set; } = Array.Empty<GlobalSearchPersonDto>();

        public IEnumerable<GlobalSearchWallDto> Walls { get; set; } = Array.Empty<GlobalSearchWallDto>();

        public IEnumerable<GlobalSearchEventDto> Events { get; set; } = Array.Empty<GlobalSearchEventDto>();

        public IEnumerable<GlobalSearchPostDto> Posts { get; set; } = Array.Empty<GlobalSearchPostDto>();

        public IEnumerable<GlobalSearchItemDto> Kudos { get; set; } = Array.Empty<GlobalSearchItemDto>();

        public IEnumerable<GlobalSearchItemDto> Projects { get; set; } = Array.Empty<GlobalSearchItemDto>();

        public IEnumerable<GlobalSearchItemDto> Groups { get; set; } = Array.Empty<GlobalSearchItemDto>();

        public IEnumerable<GlobalSearchItemDto> Books { get; set; } = Array.Empty<GlobalSearchItemDto>();

        public IEnumerable<GlobalSearchItemDto> Polls { get; set; } = Array.Empty<GlobalSearchItemDto>();

        public IEnumerable<GlobalSearchItemDto> Videos { get; set; } = Array.Empty<GlobalSearchItemDto>();

        public IEnumerable<GlobalSearchItemDto> ServiceRequests { get; set; } = Array.Empty<GlobalSearchItemDto>();
    }

    /// <summary>
    /// A result of a section that needs no more than a title, a line of detail and a date.
    /// </summary>
    public class GlobalSearchItemDto
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? Date { get; set; }

        /// <summary>
        /// Set when the result opens somewhere other than its own page: a kudos entry opens its receiver's profile.
        /// </summary>
        public string LinkId { get; set; }
    }

    public class GlobalSearchPersonDto
    {
        public string Id { get; set; }

        public string FullName { get; set; }

        public string JobTitle { get; set; }

        public string PictureId { get; set; }
    }

    public class GlobalSearchWallDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int TotalMembers { get; set; }
    }

    public class GlobalSearchPostDto
    {
        public int Id { get; set; }

        /// <summary>
        /// The text the search was found in: the post itself, or the comment under it that matched.
        /// </summary>
        public string Text { get; set; }

        public string AuthorFullName { get; set; }

        public string WallName { get; set; }

        public DateTime Created { get; set; }
    }

    public class GlobalSearchEventDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Place { get; set; }
    }
}
