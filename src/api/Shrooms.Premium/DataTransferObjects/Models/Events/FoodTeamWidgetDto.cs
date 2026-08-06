namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class FoodTeamWidgetDto
    {
        // The joined event's type, or the organization's first food event type when there is no
        // joined event. Null when the organization has no food event type configured.
        public int? EventTypeId { get; set; }

        // Null when the user has not joined a food team within the coming 8 days.
        public FoodTeamEventDto JoinedEvent { get; set; }
    }
}
