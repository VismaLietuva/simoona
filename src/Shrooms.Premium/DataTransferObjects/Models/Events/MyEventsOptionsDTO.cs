using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class MyEventsOptionsDTO : UserAndOrganizationDTO
    {
        public string SearchString { get; set; }
        public MyEventsOptions Filter { get; set; }
    }
}
