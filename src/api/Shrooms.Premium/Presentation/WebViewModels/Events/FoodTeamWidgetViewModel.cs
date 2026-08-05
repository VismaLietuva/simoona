namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class FoodTeamWidgetViewModel
    {
        // Null when the organization has no food event type configured.
        public int? EventTypeId { get; set; }

        // Null when the user has not joined a food team in the next 7 days.
        public FoodTeamEventViewModel JoinedEvent { get; set; }
    }
}
