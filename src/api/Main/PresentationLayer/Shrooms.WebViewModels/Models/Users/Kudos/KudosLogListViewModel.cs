using System;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.WebViewModels.Models.Kudos
{
    public class KudosLogListViewModel
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public ApplicationUserViewModel Employee { get; set; }

        public KudosTypeViewModel Type { get; set; }

        public int MultiplyBy { get; set; }

        public decimal Points { get; set; }

        public string Comments { get; set; }

        public string CreatedBy { get; set; }

        public ApplicationUserViewModel Creator { get; set; }

        public DateTime Created { get; set; }

        public bool Editable { get; set; }

        public bool IsApproved { get; set; }
    }
}