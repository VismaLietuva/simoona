using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class AddKudosLogDTO : UserAndOrganizationDTO
    {
        //changeName to RecievingUserId
        public IEnumerable<string> ReceivingUserIds { get; set; }
        public int PointsTypeId { get; set; }
        public int MultiplyBy { get; set; }
        public string Comment { get; set; }
        public string PictureId { get; set; }
    }
}
