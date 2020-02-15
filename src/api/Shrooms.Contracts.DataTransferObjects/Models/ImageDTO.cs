namespace Shrooms.Contracts.DataTransferObjects.Models
{
    public class ImageDTO
    {
        public byte[] Data { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}