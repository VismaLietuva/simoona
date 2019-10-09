using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.DataTransferObjects.Models.Lotteries;
using System.Linq.Expressions;
using System;
using Shrooms.EntityModels.Models;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.DataTransferObjects.Models;
using System.Threading.Tasks;
using Shrooms.Domain.Services.UserService;
using Shrooms.DomainExceptions.Exceptions.Lotteries;

namespace Shrooms.Domain.Services.Lotteries
{
    public class ParticipantService : IParticipantService
    {
        private readonly IUnitOfWork2 _unitOfWork;
        private readonly IDbSet<LotteryParticipant> _participantsDbSet;
        private readonly ILotteryService _lotteryService;
        private readonly IUserService _userService;

        public ParticipantService(IUnitOfWork2 unitOfWork, ILotteryService lotteryService, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _participantsDbSet = _unitOfWork.GetDbSet<LotteryParticipant>();
            _lotteryService = lotteryService;
            _userService = userService;
        }

        public IEnumerable<string> GetParticipantsId(int lotteryId)
        {
            return _participantsDbSet.Where(x => x.LotteryId == lotteryId).Select(x => x.UserId);
        }

        public IEnumerable<LotteryParticipantDTO> GetParticipantsCounted(int lotteryId)
        {
            return _participantsDbSet.Where(x => x.LotteryId == lotteryId)
              .GroupBy(l => l.User).Select(MapToParticipantDto());
        }

        private Expression<Func<IGrouping<ApplicationUser, LotteryParticipant>, LotteryParticipantDTO>> MapToParticipantDto()
        {
            return group => new LotteryParticipantDTO
            {
                UserId = group.Key.Id,
                FullName = group.Key.FirstName + " " + group.Key.LastName,
                Tickets = group.Distinct().Count()
            };
        }

        public async Task BuyLotteryTicketAsync(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg)
        {
            ApplicationUser applicationUser = _userService.GetApplicationUser(userOrg.UserId);

            LotteryDetailsDTO lotteryDetails = _lotteryService.GetLotteryDetails(lotteryTicketDTO.LotteryId, userOrg);

            if (applicationUser.RemainingKudos < lotteryDetails.EntryFee * lotteryTicketDTO.Tickets)
            {
                throw new LotteryException("User does not have enough kudos for the purchase.");
            }

            for (int i = 0; i < lotteryTicketDTO.Tickets; i++)
            {
                LotteryParticipant participant = MapNewLotteryParticipant(lotteryTicketDTO, userOrg);

                applicationUser.SpentKudos += lotteryDetails.EntryFee;

                applicationUser.RemainingKudos -= lotteryDetails.EntryFee;

                _participantsDbSet.Add(participant);
            }

            await _unitOfWork.SaveChangesAsync(applicationUser.Id);

        }

        private LotteryParticipant MapNewLotteryParticipant(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg)
        {
            LotteryParticipant participant = new LotteryParticipant()
            {
                LotteryId = lotteryTicketDTO.LotteryId,
                UserId = userOrg.UserId,
                Entered = DateTime.Now,
                CreatedBy = userOrg.UserId,
                ModifiedBy = userOrg.UserId
            };

            return participant;
        }
    }
}
