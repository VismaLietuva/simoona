using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Projects
{
    public class ProjectDetailsDto
    {
        public string Name { get; set; }

        public string Desc { get; set; }

        public string LogoId { get; set; }

        public virtual ApplicationUserMinimalDto Owner { get; set; }

        public virtual IEnumerable<ApplicationUserMinimalDto> Members { get; set; }

        public virtual IEnumerable<string> Attributes { get; set; }

        public int WallId { get; set; }
    }
}
