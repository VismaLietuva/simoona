using System;
using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models.User
{
    public class ApplicationUserPutJobInfoViewModel : ApplicationUserBaseViewModel
    {
        public string ManagerId { get; set; }

        public int? JobPositionId { get; set; }

        public int? QualificationLevelId { get; set; }

        public DateTime? EmploymentDate { get; set; }

        public WorkingHoursViewModel WorkingHours { get; set; }

        public IEnumerable<int> ProjectIds { get; set; }

        public IEnumerable<string> RoleIds { get; set; }

        public IEnumerable<int> SkillIds { get; set; }

        public bool IsConfirm { get; set; }
    }
}