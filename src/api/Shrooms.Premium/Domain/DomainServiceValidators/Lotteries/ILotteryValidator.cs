using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Lotteries
{
    public interface ILotteryValidator
    {
        Task CheckIfGiftReceiversExistAsync(IEnumerable<string> receiverIds, UserAndOrganizationDto userOrg);

        void CheckIfGiftedTicketLimitIsExceeded(LotteryDetailsBuyerDto buyerDto, int totalTicketCount);

        void CheckIfBuyerExists(ApplicationUser applicationUser);

        void CheckIfLotteryExists(LotteryDetailsDto detailsDto);

        void CheckIfLotteryExists(Lottery lottery);

        void CheckIfLotteryEnded(LotteryDetailsDto detailsDto);

        void CheckIfLotteryEnded(LotteryDto lotteryDto);

        void CheckIfLotteryIsDrafted(Lottery lottery);

        void CheckIfLotteryIsStarted(Lottery lottery);

        void CheckIfLotteryAllowsGifting(LotteryDetailsDto lotteryDetailsDto);

        void CheckIfUserHasEnoughKudos(ApplicationUser buyerApplicationUser, int totalTicketCost);

        bool IsValidTicketCount(BuyLotteryTicketsDto buyLotteryTicketsDto);
    }
}
