namespace Shrooms.Contracts.DataTransferObjects.Models
{
    public class ImageDto
    {
        public byte[] Data { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}