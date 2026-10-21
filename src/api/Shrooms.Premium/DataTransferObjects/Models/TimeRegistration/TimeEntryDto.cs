namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeEntryDto
    {
        public int ProjectId { get; set; }

        public int TaskId { get; set; }

        public string Date { get; set; }

        public int Minutes { get; set; }
    }
}
