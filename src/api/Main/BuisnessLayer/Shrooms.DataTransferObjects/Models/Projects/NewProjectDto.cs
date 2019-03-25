using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Projects
{
    public class NewProjectDto : UserAndOrganizationDTO
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string OwningUserId { get; set; }

        public string Logo { get; set; }

        public IEnumerable<string> MembersIds { get; set; }

        public IEnumerable<string> Attributes { get; set; }
    }
}
