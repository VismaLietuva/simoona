namespace Shrooms.Premium.DataTransferObjects.Models.Groups
{
    public class GroupReferenceDto
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public string Name { get; set; }

        public bool IsPubliclyVisible { get; set; }
    }
}
