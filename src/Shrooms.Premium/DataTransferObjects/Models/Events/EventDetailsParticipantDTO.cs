namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventDetailsParticipantDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string ImageName { get; set; }
        public int AttendStatus { get; set; }
        public string AttendComment { get; set; }
    }
}
