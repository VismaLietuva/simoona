using System;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.WebViewModels.Models.Kudos
{
    public class UserKudosInformationViewModel
    {
        public KudosTypeViewModel Type { get; set; }

        public string Comments { get; set; }

        public decimal Points { get; set; }

        public int MultiplyBy { get; set; }

        public ApplicationUserViewModel Sender { get; set; }

        public DateTime Created { get; set; }
    }
}