using System.IO;

namespace Shrooms.Contracts.DataTransferObjects.Models.Emoji
{
    public class NewCustomEmojiDto
    {
        public string Name { get; set; }

        public Stream Content { get; set; }

        public string MimeType { get; set; }

        public string FileName { get; set; }
    }
}
