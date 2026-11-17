using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Search
{
    public class GlobalSearchResultsDto
    {
        public IEnumerable<GlobalSearchPersonDto> People { get; set; } = Array.Empty<GlobalSearchPersonDto>();

        public IEnumerable<GlobalSearchWallDto> Walls { get; set; } = Array.Empty<GlobalSearchWallDto>();

        public IEnumerable<GlobalSearchEventDto> Events { get; set; } = Array.Empty<GlobalSearchEventDto>();
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

    public class GlobalSearchEventDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Place { get; set; }
    }
}
