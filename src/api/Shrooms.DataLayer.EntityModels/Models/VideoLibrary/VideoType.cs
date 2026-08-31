using System.Collections.Generic;

namespace Shrooms.DataLayer.EntityModels.Models.VideoLibrary
{
    public class VideoType : SoftDeletableModelWithOrg
    {
        public const int MaxTitleLength = 50;

        public string Title { get; set; }

        public virtual ICollection<VideoLibraryItem> Videos { get; set; }
    }
}
