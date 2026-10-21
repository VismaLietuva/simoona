namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeTaskInputDto
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string Comment { get; set; }

        public bool IsDefault { get; set; }
    }
}
