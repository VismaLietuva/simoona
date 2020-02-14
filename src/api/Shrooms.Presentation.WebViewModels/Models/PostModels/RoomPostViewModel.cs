using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Presentation.WebViewModels.Models.User;

namespace Shrooms.Presentation.WebViewModels.Models.PostModels
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