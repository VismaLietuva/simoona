using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Lotteries
{
    public interface ILotteryAbortService
    {
        void RefundLottery(Lottery lottery, UserAndOrganizationDTO userOrg);
    }
}
