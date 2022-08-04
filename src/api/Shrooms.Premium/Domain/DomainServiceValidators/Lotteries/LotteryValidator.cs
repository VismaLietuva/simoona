using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Lotteries
{
    public class LotteryValidator : ILotteryValidator
    {
        private readonly IDbSet<LotteryParticipant> _lotteryParticipantsDbSet;

        private readonly ISystemClock _systemClock;

        public LotteryValidator(ISystemClock systemClock, IUnitOfWork2 uow)
        {
            _systemClock = systemClock;
            _lotteryParticipantsDbSet = uow.GetDbSet<LotteryParticipant>();
        }

        public async Task CheckIfGiftedTicketLimitIsExceededAsync(ApplicationUser buyer, LotteryDetailsDto detailsDto, BuyLotteryTicketsDto buyDto)
        {
            var totalTicketCount = buyDto.ReceivingUserIds.Length * buyDto.Tickets;
            
            var exception = new LotteryException($"Cannot gift more than {detailsDto.GiftedTicketLimit} tickets");

            if (detailsDto.GiftedTicketLimit < totalTicketCount)
            {
                throw exception;
            }

            // Add participant collection to lottery?
            var previouslyBoughtTicketCount = await _lotteryParticipantsDbSet
                .Include(participant => participant.Lottery)
                .CountAsync(participant => participant.LotteryId == detailsDto.Id &&
                                           participant.Lottery.OrganizationId == buyer.OrganizationId &&
                                           participant.CreatedBy == buyer.Id);

            totalTicketCount += previouslyBoughtTicketCount;

            if (detailsDto.GiftedTicketLimit < totalTicketCount)
            {
                throw exception;
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
                CheckIfLotteryExists((Lottery)null);
            }
        }

        public void CheckIfLotteryExists(Lottery lottery)
        {
            if (lottery == null)
            {
                throw new LotteryException("Lottery was not found");
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

        public void CheckIfReceivingUsersExist(BuyLotteryTicketsDto buyLotteryTicketsDto)
        {
            if (buyLotteryTicketsDto.ReceivingUserIds?.Length != 0)
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
            return buyLotteryTicketsDto.Tickets > 0;
        }

        private void CheckIfLotteryEnded(DateTime endDate, string errorMessage)
        {
            if (_systemClock.UtcNow > endDate)
            {
                throw new LotteryException(errorMessage);
            }
        }
    }
}
