using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class MyEventsOptionsDTO : UserAndOrganizationDTO
    {
        public string SearchString { get; set; }
        public MyEventsOptions Filter { get; set; }
    }
}
