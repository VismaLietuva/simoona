using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shrooms.WebViewModels.Models
{
    public class FloorViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string Map { get; set; }

        public HttpPostedFileBase PostedMapPicture { get; set; }

        public int OfficeId { get; set; }

        public OfficeViewModel Office { get; set; }

        public IEnumerable<RoomViewModel> Rooms { get; set; }

        public string PictureId { get; set; }

        public int RoomsCount => Rooms?.Count() ?? 0;

        public string OrganizationName { get; set; }

        public int ApplicationUsersCount { get; set; }
    }

    public class FloorViewPagedModel : PagedViewModel<FloorViewModel>
    {
        public OfficeViewModel Office { get; set; }
    }
}