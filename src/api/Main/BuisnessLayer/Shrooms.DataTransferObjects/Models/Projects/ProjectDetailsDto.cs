using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Projects
{
    public class ProjectDetailsDto
    {
        public string Name { get; set; }

        public string Desc { get; set; }

        public string LogoId { get; set; }

        public virtual ApplicationUserMinimalViewModelDto Owner { get; set; }

        public virtual IEnumerable<ApplicationUserMinimalViewModelDto> Members { get; set; }

        public virtual IEnumerable<string> Attributes { get; set; }

        public int WallId { get; set; }
    }
}
