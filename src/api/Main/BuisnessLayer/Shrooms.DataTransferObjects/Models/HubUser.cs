using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models
{
    public class HubUser
    {
        public string Id { get; set; }

        public int OrganizationId { get; set; }

        public string OrganizationName { get; set; }

        public HashSet<string> ConnectionIds { get; set; }
    }
}
