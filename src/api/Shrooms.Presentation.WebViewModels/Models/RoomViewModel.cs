using System.Collections.Generic;
using System.Linq;
using Shrooms.Presentation.WebViewModels.Models.User;

namespace Shrooms.Presentation.WebViewModels.Models
{
    public class RoomViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string Number { get; set; }

        public string Coordinates { get; set; }

        public int FloorId { get; set; }

        public FloorViewModel Floor { get; set; }

        public IEnumerable<ApplicationUserViewModel> ApplicationUsers { get; set; }

        public OfficeViewModel Office { get; set; }

        public int ApplicationUsersCount
        {
            get
            {
                return this.ApplicationUsers != null ? this.ApplicationUsers.Count() : 0;
            }
        }

        public int? RoomTypeId { get; set; }

        public RoomTypeViewModel RoomType { get; set; }
    }
}