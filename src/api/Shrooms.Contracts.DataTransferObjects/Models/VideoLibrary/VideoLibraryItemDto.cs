using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary
{
    public class VideoLibraryItemDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public int? VideoTypeId { get; set; }

        public string VideoTypeTitle { get; set; }

        public DateTime Created { get; set; }
    }
}
