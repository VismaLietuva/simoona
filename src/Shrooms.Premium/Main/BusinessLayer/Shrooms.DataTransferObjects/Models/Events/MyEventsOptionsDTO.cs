using Shrooms.Constants.BusinessLayer;
using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Events
{
    public class MyEventsOptionsDTO : UserAndOrganizationDTO
    {
        public string SearchString { get; set; }
        public BusinessLayerConstants.MyEventsOptions Filter { get; set; }
    }
}
