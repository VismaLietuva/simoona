using Shrooms.DataTransferObjects.Models;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events
{
    public class MyEventsOptionsDTO : UserAndOrganizationDTO
    {
        public string SearchString { get; set; }
        public MyEventsOptions Filter { get; set; }
    }
}
