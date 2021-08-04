using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Kudos
{
    public class AddKudosLogDto : UserAndOrganizationDto
    {
        public IEnumerable<string> ReceivingUserIds { get; set; }
        public int PointsTypeId { get; set; }
        public int MultiplyBy { get; set; }
        public string Comment { get; set; }
        public string PictureId { get; set; }
        public bool IsActive { get; set; }
    }
}
