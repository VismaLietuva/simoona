﻿namespace Shrooms.Contracts.DataTransferObjects
{
    public class ApplicationUserMinimalDto
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string PictureId { get; set; }

        public string JobPosition { get; set; }
    }
}
