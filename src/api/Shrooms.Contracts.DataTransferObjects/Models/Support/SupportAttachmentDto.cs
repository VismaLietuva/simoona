namespace Shrooms.Contracts.DataTransferObjects.Models.Support
{
    /// <summary>
    /// An optional screenshot submitted with a support ticket. Held in memory and
    /// attached to the outgoing email — support attachments are never persisted to
    /// blob storage the way pictures are.
    /// </summary>
    public class SupportAttachmentDto
    {
        public byte[] Content { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }
    }
}
