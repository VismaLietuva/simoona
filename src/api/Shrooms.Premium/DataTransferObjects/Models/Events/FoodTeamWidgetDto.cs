namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class FoodTeamWidgetDto
    {
        // Null when the organization has no food event type configured.
        public int? EventTypeId { get; set; }

        // Null when the user has not joined a food team in the next 7 days.
        public FoodTeamEventDto JoinedEvent { get; set; }
    }
}
