namespace Shrooms.Contracts.DataTransferObjects
{
    public class EmailAttachment
    {
        public byte[] Content { get; }
        public string FileName { get; }
        public string ContentType { get; }

        public EmailAttachment(byte[] content, string fileName, string contentType)
        {
            Content = content;
            FileName = fileName;
            ContentType = contentType;
        }
    }
}
