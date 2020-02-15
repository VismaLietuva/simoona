using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Administration
{
    public class AdministrationUserDTO
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string JobTitle { get; set; }

        public bool IsNewUser { get; set; }

        public bool HasRoom { get; set; }

        public ICollection<AdministrationProjectDTO> Projects { get; set; }

        public ICollection<AdministrationSkillDTO> Skills { get; set; }
    }
}
