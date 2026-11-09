namespace Shrooms.DataLayer.EntityModels.Models.VideoLibrary
{
    public class VideoLibraryMaterial : BaseModel
    {
        public const int MaxTitleLength = 100;
        public const int MaxUrlLength = 2000;
        public const int MaxPerVideo = 10;

        public int VideoLibraryItemId { get; set; }

        public virtual VideoLibraryItem VideoLibraryItem { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public int SortOrder { get; set; }
    }
}
