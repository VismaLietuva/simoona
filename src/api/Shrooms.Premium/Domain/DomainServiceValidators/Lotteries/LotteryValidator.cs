using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Lotteries
{
    public class LotteryValidator : ILotteryValidator
    {
        private readonly ISystemClock _systemClock;

        private readonly DbSet<ApplicationUser> _usersDbSet;

        public LotteryValidator(ISystemClock systemClock, IUnitOfWork2 uow)
        {
            _systemClock = systemClock;

            _usersDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public async Task CheckIfGiftReceiversExistAsync(IEnumerable<string> receiverIds, UserAndOrganizationDto userOrg)
        {
            var foundReceiverIds = await _usersDbSet
                .Where(user => receiverIds.Contains(user.Id) &&
                               user.OrganizationId == userOrg.OrganizationId)
                .Select(user => user.Id)
                .ToListAsync();

            if (foundReceiverIds.Count != receiverIds.Count())
            {
                throw new LotteryException("Provided receivers were not found");
            }

            if (foundReceiverIds.Contains(userOrg.UserId))
            {
                throw new LotteryException("Cannot gift tickets to yourself");
            }
        }

        public void CheckIfGiftedTicketLimitIsExceeded(LotteryDetailsBuyerDto buyerDto, int totalTicketCount)
        {
            if (buyerDto.RemainingGiftedTicketCount == 0)
            {
                throw new LotteryException($"Reached ticket limit");
            }

            if (buyerDto.RemainingGiftedTicketCount < totalTicketCount)
            {
                throw new LotteryException($"Can only gift {buyerDto.RemainingGiftedTicketCount} tickets");
            }
        }

        public void CheckIfBuyerExists(ApplicationUser buyerApplicationUser)
        {
            if (buyerApplicationUser == null)
            {
                throw new LotteryException("Buyer was not found");
            }
        }

        public void CheckIfLotteryExists(LotteryDetailsDto detailsDto)
        {
            if (detailsDto == null)
            {
                throw GetLotteryNotFoundException();
            }
        }

        public void CheckIfLotteryExists(Lottery lottery)
        {
            if (lottery == null)
            {
                throw GetLotteryNotFoundException();
            }
        }

        public void CheckIfLotteryEnded(LotteryDetailsDto detailsDto)
        {
            CheckIfLotteryEnded(detailsDto.EndDate, "Lottery has already ended");
        }

        public void CheckIfLotteryEnded(LotteryDto lotteryDto)
        {
            CheckIfLotteryEnded(lotteryDto.EndDate, "Lottery can't start in the past.");
        }

        public void CheckIfLotteryAllowsGifting(LotteryDetailsDto lotteryDetailsDto)
        {
            if (lotteryDetailsDto.GiftedTicketLimit <= 0)
            {
                throw new LotteryException("This lottery does not allow gifting tickets");
            }
        }

        public void CheckIfUserHasEnoughKudos(ApplicationUser buyerApplicationUser, int totalTicketCost)
        {
            if (buyerApplicationUser.RemainingKudos < totalTicketCost)
            {
                throw new LotteryException("User does not have enough kudos for the purchase.");
            }
        }

        public void CheckIfLotteryIsDrafted(Lottery lottery)
        {
            if (lottery.Status != LotteryStatus.Drafted)
            {
                throw new LotteryException("Lottery has to be drafted");
            }
        }

        public void CheckIfLotteryIsStarted(Lottery lottery)
        {
            if (lottery.Status != LotteryStatus.Started)
            {
                throw new LotteryException("Lottery has to be started");
            }
        }

        public bool IsValidTicketCount(BuyLotteryTicketsDto buyLotteryTicketsDto)
        {
            return buyLotteryTicketsDto.TicketCount > 0;
        }

        private void CheckIfLotteryEnded(DateTime endDate, string errorMessage)
        {
            if (_systemClock.UtcNow > endDate)
            {
                throw new LotteryException(errorMessage);
            }
        }

        private static LotteryException GetLotteryNotFoundException()
        {
            return new LotteryException("Lottery was not found");
        }
    }
}
