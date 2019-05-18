using Shrooms.DataTransferObjects.Models;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events
{
    public class MyEventsOptionsDTO : UserAndOrganizationDTO
    {
        public string SearchString { get; set; }
        public BusinessLayerConstants.MyEventsOptions Filter { get; set; }
    }
}
