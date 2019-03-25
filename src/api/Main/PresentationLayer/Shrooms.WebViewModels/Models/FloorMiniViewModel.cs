namespace Shrooms.WebViewModels.Models
{
    public class FloorMiniViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string Map { get; set; }

        public int OfficeId { get; set; }

        public OfficeMiniViewModel Office { get; set; }

        public string PictureId { get; set; }
    }
}