using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.WebViewModels.Models
{
    public class RoomPostViewModel : AbstractViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Number { get; set; }

        public string Coordinates { get; set; }

        public int FloorId { get; set; }

        public AbstractViewModel Floor { get; set; }

        public IEnumerable<ApplicationUserViewModel> ApplicationUsers { get; set; }

        public AbstractViewModel Office { get; set; }

        public int? RoomTypeId { get; set; }

        public AbstractViewModel RoomType { get; set; }
    }
}