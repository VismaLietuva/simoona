using System;
using System.Collections.Generic;
using Shrooms.Contracts.ViewModels.Wall.Likes;

namespace Shrooms.Presentation.WebViewModels.Models.Users.Kudos
{
    public class WallKudosLogViewModel
    {
        public int Id { get; set; }

        public KudosLogUserViewModel Sender { get; set; }

        public KudosLogUserViewModel Receiver { get; set; }

        public decimal Points { get; set; }

        public string Comment { get; set; }

        public DateTime Created { get; set; }

        public string PictureId { get; set; }

        public IEnumerable<LikeViewModel> Likes { get; set; }
    }
}
