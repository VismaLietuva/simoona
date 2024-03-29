﻿namespace Shrooms.Premium.DataTransferObjects.Models.OrganizationalStructure
{
    public class OrganizationalStructureUserDto
    {
        public string Id { get; set; }

        public string ManagerId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PictureId { get; set; }

        public bool IsManagingDirector { get; set; }
    }
}
