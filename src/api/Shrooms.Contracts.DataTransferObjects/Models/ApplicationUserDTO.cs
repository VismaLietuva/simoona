using System;

namespace Shrooms.Contracts.DataTransferObjects.Models
{
    public class ApplicationUserDTO
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string UserName { get; set; }

        public string Email { get; set; }

        public string PictureId { get; set; }

        public DateTime? EmploymentDate { get; set; }

        public decimal TotalKudos { get; set; }

        public decimal RemainingKudos { get; set; }

        public decimal SpentKudos { get; set; }
    }
}