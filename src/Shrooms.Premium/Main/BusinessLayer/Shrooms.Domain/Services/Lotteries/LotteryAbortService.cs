using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.Infrastructure.FireAndForget;

namespace Shrooms.Domain.Services.Lotteries
{
    public class LotteryAbortService : ILotteryAbortService
    {
        private readonly IParticipantService _participantService;

        private readonly IKudosService _kudosService;

        public LotteryAbortService(IKudosService kudosService, IParticipantService participantService)
        {
            _kudosService = kudosService;
            _participantService = participantService;
        }

        public void RefundLottery(Lottery lottery, UserAndOrganizationDTO userOrg)
        {
            var usersToRefund = _participantService.GetParticipantsToRefund(lottery.Id);

            foreach (var user in usersToRefund)
            {
                _participantService.SetTicketsAsRefunded(lottery.Id, user.UserId);

                var kudosLog = new AddKudosLogDTO
                {
                    ReceivingUserIds = new List<string> { user.UserId },
                    PointsTypeId = 3,
                    MultiplyBy = user.Tickets * lottery.EntryFee,
                    Comment = $"Refund for lottery {lottery.Title}",
                    UserId = user.UserId,
                    OrganizationId = userOrg.OrganizationId
                };

                _kudosService.RefundLotteryTicket(kudosLog, userOrg);
            }
        }
    }
}
