using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using System;

namespace Shrooms.Contracts.DataTransferObjects.Employees
{
    public class EmployeeDto
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? BirthDay { get; set; }

        public string JobTitle { get; set; }

        public string PhoneNumber { get; set; }

        public WorkingHourslWithOutLunchDto WorkingHours { get; set; }

        public BlacklistStateDto BlacklistState { get; set; }
    }
}
