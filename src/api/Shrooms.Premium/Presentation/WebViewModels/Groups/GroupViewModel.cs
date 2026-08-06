using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.ViewModels;
using Shrooms.Contracts.ViewModels.User;

namespace Shrooms.Premium.Presentation.WebViewModels.Groups
{
    public class GroupViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public int GroupTypeId { get; set; }

        public string GroupTypeName { get; set; }

        public GroupTypeViewModel GroupType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public GroupStatus Status { get; set; }

        public bool IsPending { get; set; }

        public string CreatedBy { get; set; }

        public string ApprovalAnswers { get; set; }

        public bool IsExpired { get; set; }

        public ICollection<GroupMemberViewModel> Members { get; set; }

        public ICollection<GroupReferenceViewModel> References { get; set; }
    }
}
