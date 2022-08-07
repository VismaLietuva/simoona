using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Lotteries
{
    public interface ILotteryValidator
    {
        Task CheckIfGiftReceiversExistAsync(BuyLotteryTicketsDto buyTicketsDto, UserAndOrganizationDto userOrg);

        void CheckIfGiftedTicketLimitIsExceeded(LotteryDetailsBuyerDto buyerDto, BuyLotteryTicketsDto buyLotteryDto);

        void CheckIfBuyerExists(ApplicationUser applicationUser);

        void CheckIfLotteryExists(LotteryDetailsDto detailsDto);

        void CheckIfLotteryExists(Lottery lottery);

        void CheckIfLotteryEnded(LotteryDetailsDto detailsDto);

        void CheckIfLotteryEnded(LotteryDto lotteryDto);

        void CheckIfLotteryIsDrafted(Lottery lottery);

        void CheckIfLotteryIsStarted(Lottery lottery);

        void CheckIfGiftedTicketsReceiversExist(BuyLotteryTicketsDto buyLotteryTicketsDto);

        void CheckIfUserHasEnoughKudos(ApplicationUser buyerApplicationUser, int totalTicketCost);

        bool IsValidTicketCount(BuyLotteryTicketsDto buyLotteryTicketsDto);
    }
}
