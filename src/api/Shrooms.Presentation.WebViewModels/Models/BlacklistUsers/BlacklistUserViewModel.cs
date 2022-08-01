using Shrooms.Contracts.Enums;
using System;

namespace Shrooms.Presentation.WebViewModels.Models.BlacklistUsers
{
    public class BlacklistUserViewModel
    {
        public string UserId { get; set; }

        public DateTime EndDate { get; set; }
        
        public string Reason { get; set; }

        public DateTime Modified { get; set; }

        public string ModifiedBy { get; set; }

        public string ModifiedByUserFirstName { get; set; }

        public string ModifiedByUserLastName { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedByUserFirstName { get; set; }

        public string CreatedByUserLastName { get; set; }

        public BlacklistStatus Status { get; set; }
    }
}