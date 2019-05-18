namespace Shrooms.WebViewModels.Models.ChangeProfile
{
    public class ChangeProfileOfficeViewModel : ChangeProfileBaseModel
    {
        public int? RoomId { get; set; }

        public RoomViewModel Room { get; set; }
    }
}