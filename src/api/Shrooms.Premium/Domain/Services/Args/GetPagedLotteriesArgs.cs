using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Domain.Services.Args
{
    public class GetPagedLotteriesArgs
    {
        public string Filter { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public UserAndOrganizationDto UserOrg { get; set; }
    }
}
