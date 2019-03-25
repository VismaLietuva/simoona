using System;

namespace Shrooms.DataTransferObjects.Models.Birthdays
{
    public class UserBirthdayInfoDTO
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? BirthDay { get; set; }

        public string PictureId { get; set; }
    }
}
