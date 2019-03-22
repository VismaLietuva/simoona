namespace Shrooms.DataTransferObjects.Models.Events
{
    public class EventTypeDTO
    {
        public int Id { get; set; }

        public bool IsSingleJoin { get; set; }

        public string Name { get; set; }

        public bool HasActiveEvents { get; set; }
    }
}
