namespace Shrooms.WebViewModels.Models
{
    public class ChangeProfileOfficeViewModel : ChangeProfileBaseModel
    {
        public int? RoomId { get; set; }

        public RoomViewModel Room { get; set; }
    }
}