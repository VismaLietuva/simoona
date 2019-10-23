using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Domain.Services.Args
{
    public class GetPagedLotteriesArgs
    {
        public string Filter { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public UserAndOrganizationDTO UserOrg { get; set; }
    }
}
