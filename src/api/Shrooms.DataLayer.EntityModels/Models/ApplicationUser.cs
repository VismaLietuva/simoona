﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models.Badges;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Notifications;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class ApplicationUser : IdentityUser, ISoftDelete, ITrackable, IOrganization
    {
        public const int MaxPasswordLength = 100;

        public const int MinPasswordLength = 6;

        public ApplicationUser()
        {
            Created = DateTime.UtcNow;
            Modified = DateTime.UtcNow;
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public string Bio { get; set; }

        public DateTime? EmploymentDate { get; set; }

        public DateTime? BirthDay { get; set; }

        public int? WorkingHoursId { get; set; }

        public virtual WorkingHours WorkingHours { get; set; }

        public bool IsAbsent { get; set; }

        public bool IsAnonymized { get; set; }

        public string AbsentComment { get; set; }

        public decimal TotalKudos { get; set; }

        public decimal RemainingKudos { get; set; }

        public int SittingPlacesChanged { get; set; }

        public decimal SpentKudos { get; set; }

        [ForeignKey("Room")]
        public int? RoomId { get; set; }

        public virtual Room Room { get; set; }

        public string PictureId { get; set; }

        [ForeignKey("QualificationLevel")]
        public int? QualificationLevelId { get; set; }

        public virtual QualificationLevel QualificationLevel { get; set; }

        public string ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        public virtual ApplicationUser Manager { get; set; }

        public virtual ICollection<ApplicationUser> ManagedUsers { get; set; }

        public virtual ICollection<Committee.Committee> Committees { get; set; }

        [InverseProperty("Leads")]
        public virtual ICollection<Committee.Committee> LeadingCommittees { get; set; }

        [InverseProperty("Delegates")]
        public virtual ICollection<Committee.Committee> DelegatingCommittees { get; set; }

        public int OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        public bool IsOwner { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime Modified { get; set; }

        public string ModifiedBy { get; set; }

        public virtual ICollection<Exam> Exams { get; set; }

        public virtual ICollection<Certificate> Certificates { get; set; }

        public virtual ICollection<Skill> Skills { get; set; }

        public virtual ICollection<Book> Books { get; set; }

        public virtual ICollection<BookLog> BookLogs { get; set; }

        public virtual ICollection<BadgeLog> BadgeLogs { get; set; }

        public virtual ICollection<Event> Events { get; set; }

        public virtual ICollection<WallMember> WallUsers { get; set; }

        public virtual ICollection<ServiceRequestCategory> ServiceRequestCategoriesAssigned { get; set; }

        public double? VacationTotalTime { get; set; }

        public double? VacationUsedTime { get; set; }

        public double? VacationUnusedTime { get; set; }

        public DateTime? VacationLastTimeUpdated { get; set; }

        public TimeSpan? DailyMailingHour { get; set; }

        public bool IsManagingDirector { get; set; }

        public string CultureCode { get; set; }

        public virtual ICollection<Project> Projects { get; set; }

        public virtual ICollection<Project> OwnedProjects { get; set; }

        public int? JobPositionId { get; set; }

        [ForeignKey("JobPositionId")]
        public virtual JobPosition JobPosition { get; set; }

        public string TimeZone { get; set; }

        public virtual ICollection<NotificationUser> NotificationUsers { get; set; }

        public bool IsTutorialComplete { get; set; }

        public virtual NotificationsSettings NotificationsSettings { get; set; }

        public string GoogleEmail { get; set; }

        public string FacebookEmail { get; set; }

        [NotMapped]
        public int YearsEmployed
        {
            get
            {
                var now = DateTime.UtcNow;
                EmploymentDate ??= now;

                var employmentYears = now.Year - EmploymentDate.Value.Year;
                if (now < EmploymentDate.Value.AddYears(employmentYears))
                {
                    employmentYears = employmentYears >= 1 ? employmentYears - 1 : employmentYears;
                }

                return employmentYears;
            }
        }

        public void ReceiveKudos(KudosLog log)
        {
            RemainingKudos += log.Points;
            TotalKudos += log.Points;
            Modified = DateTime.UtcNow;
        }
    }
}