using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.EntityModels.Models.Lotteries;

namespace Shrooms.Domain.Services.Lotteries
{
    public class LotteryAbortJob : ILotteryAbortJob
    {
        private readonly IParticipantService _participantService;

        private readonly IKudosService _kudosService;

        public LotteryAbortJob(IKudosService kudosService, IParticipantService participantService)
        {
            _kudosService = kudosService;
            _participantService = participantService;
        }

        public void RefundLottery(Lottery lottery, UserAndOrganizationDTO userOrg)
        {
            var usersToRefund = _participantService.GetParticipantsToRefund(lottery.Id);

            foreach (var user in usersToRefund)
            {
                var totalReturn = user.Tickets * lottery.EntryFee;
                var kudosLog = new AddKudosLogDTO
                {
                    ReceivingUserIds = new List<string> { user.UserId },
                    PointsTypeId = 3,
                    MultiplyBy = totalReturn,
                    Comment = FormatComment(lottery, totalReturn),
                    UserId = user.UserId,
                    OrganizationId = userOrg.OrganizationId
                };

                _kudosService.RefundLotteryTicket(kudosLog, userOrg);
                _participantService.SetTicketsAsRefunded(lottery.Id, user.UserId);
            }
        }

        private string FormatComment(Lottery lottery, int total)
        {
            return $"Refund for lottery {lottery.Title}. Returned {total} kudos.";
        }
    }
}