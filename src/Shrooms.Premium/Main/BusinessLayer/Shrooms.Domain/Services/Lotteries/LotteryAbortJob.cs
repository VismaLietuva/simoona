using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.Logger;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.Domain.Services.Lotteries
{
    public class LotteryAbortJob : ILotteryAbortJob
    {
        private readonly IParticipantService _participantService;

        private readonly IKudosService _kudosService;

        private readonly ILotteryService _lotteryService;

        private readonly IAsyncRunner _asyncRunner;

        private readonly ILogger _logger;

        private readonly IUnitOfWork2 _uow;

        public LotteryAbortJob(IKudosService kudosService, IParticipantService participantService,
            ILogger logger, ILotteryService lotteryService, IAsyncRunner asyncRunner, IUnitOfWork2 uow)
        {
            _kudosService = kudosService;
            _participantService = participantService;
            _logger = logger;
            _lotteryService = lotteryService;
            _asyncRunner = asyncRunner;
            _uow = uow;
        }

        public void RefundLottery(Lottery lottery, UserAndOrganizationDTO userOrg)
        {
            var usersToRefund = _participantService.GetParticipantsCounted(lottery.Id);
            var usersToSendKudos = new List<AddKudosLogDTO>();

            foreach (var user in usersToRefund)
            {
                var totalReturn = user.Tickets * lottery.EntryFee;
                var kudosLog = new AddKudosLogDTO
                {
                    ReceivingUserIds = new List<string> { user.UserId },
                    PointsTypeId = 5,
                    MultiplyBy = totalReturn,
                    Comment = FormatComment(lottery, totalReturn),
                    UserId = userOrg.UserId,
                    OrganizationId = userOrg.OrganizationId
                };
                usersToSendKudos.Add(kudosLog);
            }

            try
            {
                _kudosService.RefundLotteryTickets(usersToSendKudos, userOrg);
                _lotteryService.EditLotteryStatus(lottery.Id, LotteryStatus.Aborted);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                _asyncRunner.Run<ILotteryService>(n => n.EditLotteryStatus(lottery.Id, LotteryStatus.RefundFailed), _uow.ConnectionName);
            }
        }

        private string FormatComment(Lottery lottery, int total)
        {
            return $"Refund for lottery {lottery.Title}. Returned {total} kudos.";
        }
    }
}