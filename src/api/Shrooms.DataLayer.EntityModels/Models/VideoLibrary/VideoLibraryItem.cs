namespace Shrooms.DataLayer.EntityModels.Models.VideoLibrary
{
    public class VideoLibraryItem : SoftDeletableModelWithOrg
    {
        public const int MaxTitleLength = 200;
        public const int MaxUrlLength = 2000;
        public const int MaxDescriptionLength = 1000;
        public const int MaxPictureIdLength = 100;

        public string Title { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public int? VideoTypeId { get; set; }

        public virtual VideoType VideoType { get; set; }
    }
}
