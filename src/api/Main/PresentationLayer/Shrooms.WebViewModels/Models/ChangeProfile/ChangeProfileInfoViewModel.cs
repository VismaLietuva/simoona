using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Shrooms.EntityModels.Models;

namespace Shrooms.WebViewModels.Models
{
    public class ChangeProfileInfoViewModel : ChangeProfileBaseModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MessageRequiredField")]
        [DataMember(IsRequired = true)]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MessageRequiredField")]
        [DataMember(IsRequired = true)]
        public string LastName { get; set; }

        [StringLength(255, MinimumLength = 3)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string JobTitle { get; set; }

        public string Skills { get; set; }

        public string Bio { get; set; }

        public DateTime? EmploymentDate { get; set; }

        public int? QualificationLevelId { get; set; }

        public QualificationLevel QualificationLevel { get; set; }

        public bool IsAbsent { get; set; }

        public string AbsentComment { get; set; }

        public string PictureId { get; set; }

        public OrganizationViewModel Organization { get; set; }

        public int OrganizationId { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime? Birthday { get; set; }

        public WorkingHoursViewModel WorkingHours { get; set; }

        public IEnumerable<ApplicationRoleViewModel> Roles { get; set; }

        public IEnumerable<CertificateViewModel> Certificates { get; set; }
    }
}