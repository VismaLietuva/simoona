using System;
using System.Collections.Generic;
using Shrooms.Presentation.WebViewModels.Models.Certificate;
using Shrooms.Presentation.WebViewModels.Models.Exam;
using Shrooms.Presentation.WebViewModels.Models.Profile.JobPosition;
using Shrooms.Presentation.WebViewModels.Models.Projects;
using Shrooms.Presentation.WebViewModels.Models.Skill;

namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class ApplicationUserJobInfoViewModel : ApplicationUserBaseViewModel
    {
        public IEnumerable<JobPositionViewModel> JobPositions { get; set; }

        public ManagerMiniViewModel Manager { get; set; }

        public IEnumerable<ProjectsBasicInfoViewModel> Projects { get; set; }

        public IEnumerable<SkillMiniViewModel> Skills { get; set; }

        public QualificationLevelMiniViewModel QualificationLevel { get; set; }

        public IEnumerable<CertificateMiniViewModel> Certificates { get; set; }

        public IEnumerable<ExamMiniViewModel> Exams { get; set; }

        public IEnumerable<ApplicationRoleMiniViewModel> Roles { get; set; }

        public DateTime? EmploymentDate { get; set; }

        public WorkingHoursViewModel WorkingHours { get; set; }

        public bool IsAbsent { get; set; }

        public string AbsentComment { get; set; }
    }
}