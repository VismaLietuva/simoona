using System;

namespace Shrooms.Presentation.WebViewModels.Models
{
    public class EmployeeListViewModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? BirthDay { get; set; }

        public string JobTitle { get; set; }

        public string PhoneNumber { get; set; }

        public WorkingHourslWithOutLunchViewModel WorkingHours { get; set; }
    }

    public class WorkingHourslWithOutLunchViewModel
    {
        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }
    }
}