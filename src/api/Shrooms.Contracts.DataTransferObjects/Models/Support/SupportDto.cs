using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Support
{
    public class SupportDto
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public SupportType Type { get; set; }
        public IList<SupportAttachmentDto> Attachments { get; set; } = new List<SupportAttachmentDto>();
    }
}