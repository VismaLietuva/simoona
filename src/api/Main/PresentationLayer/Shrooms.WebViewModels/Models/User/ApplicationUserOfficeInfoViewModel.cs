namespace Shrooms.WebViewModels.Models.User
{
    public class ApplicationUserOfficeInfoViewModel : ApplicationUserBaseViewModel
    {
        public int? RoomId { get; set; }

        public RoomMiniViewModel Room { get; set; }
    }
}