using System;
using System.Collections.Generic;
using System.Web;
using Shrooms.Presentation.WebViewModels.Models.Skill;

namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class ApplicationUserViewModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string JobTitle { get; set; }

        public IEnumerable<SkillMiniViewModel> Skills { get; set; }

        public string Bio { get; set; }

        public DateTime? EmploymentDate { get; set; }

        public bool IsAbsent { get; set; }

        public string AbsentComment { get; set; }

        public int? RoomId { get; set; }

        public RoomViewModel Room { get; set; }

        public string PictureId { get; set; }

        public OrganizationViewModel Organization { get; set; }

        public int OrganizationId { get; set; }

        public decimal TotalKudos { get; set; }

        public string SecurityStamp { get; set; }

        public HttpPostedFileBase PostedUserPhoto { get; set; }

        public string Map { get; set; }

        public int? QualificationLevelId { get; set; }

        public QualificationLevelViewModel QualificationLevel { get; set; }

        public string QualificationLevelName
        {
            get
            {
                return QualificationLevel == null ? null : QualificationLevel.Name;
            }
        }

        public IEnumerable<ApplicationRoleViewModel> Roles { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public bool IsAdmin { get; set; }

        public bool IsNewUser { get; set; }

        public bool HasRoom
        {
            get { return Room != null || RoomId != null; }
        }
    }

    public class ApplicationUserViewPagedModel : PagedViewModel<ApplicationUserViewModel>
    {
        public int RoomId { get; set; }

        public RoomViewModel Room { get; set; }

        public FloorViewModel Floor { get; set; }

        public OfficeViewModel Office { get; set; }
    }
}