using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Lotteries
{
    public interface ILotteryService
    {
        Task<CreateLotteryDTO> CreateLottery(CreateLotteryDTO newLotteryDTO);
        void EditDraftedLottery(EditDraftedLotteryDTO lotteryDTO);
        void EditStartedLottery(EditStartedLotteryDTO lotteryDTO);
        void RemoveLottery(Guid id, UserAndOrganizationDTO userOrg);
        LotteryDetailsDTO GetLotteryDetails(Guid id, UserAndOrganizationDTO userOrg);
    }
}
