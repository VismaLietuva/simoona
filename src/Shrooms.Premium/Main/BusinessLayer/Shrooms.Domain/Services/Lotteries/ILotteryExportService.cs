using Shrooms.DataTransferObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Lotteries
{
    public interface ILotteryExportService
    {
        byte[] ExportParticipants(int lotteryId, UserAndOrganizationDTO userAndOrg);
    }
}
