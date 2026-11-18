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
