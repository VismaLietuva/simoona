using System;

namespace Shrooms.Premium.Presentation.WebViewModels.Groups
{
    public class GroupMemberViewModel
    {
        public int MembershipId { get; set; }

        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string PictureId { get; set; }

        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
