namespace Shrooms.Contracts.DataTransferObjects
{
    public class FileExportDto
    {
        public FileExportDto(byte[] content, string fileName)
        {
            Content = content;
            FileName = fileName;
        }

        public byte[] Content { get; }

        public string FileName { get; }
    }
}
