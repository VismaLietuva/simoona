namespace Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary
{
    public class VideoTypeDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int VideoCount { get; set; }
    }
}
