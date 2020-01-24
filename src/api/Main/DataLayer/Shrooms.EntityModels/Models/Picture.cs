namespace Shrooms.EntityModels.Models
{
    public class Picture : BaseModelWithOrg
    {
        public string Name { get; set; }

        public string MimeType { get; set; }

        public byte[] ImageData { get; set; }

        public byte[] ThumbData { get; set; }

        public byte[] MiniThumbData { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? ThumbWidth { get; set; }

        public int? ThumbHeight { get; set; }

        public int? MiniThumbWidth { get; set; }

        public int? MiniThumbHeight { get; set; }
    }
}