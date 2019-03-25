using Shrooms.EntityModels.Models;

namespace Shrooms.DataTransferObjects.Models.Support
{
    public class SupportDto
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public SupportType Type { get; set; }
    }
}