using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Constants.BusinessLayer;
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

        public LotteryAbortJob(
            IKudosService kudosService,
            IParticipantService participantService,
            ILogger logger,
            ILotteryService lotteryService,
            IAsyncRunner asyncRunner,
            IUnitOfWork2 uow)
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
            try
            {
                var refundLogs = CreateKudosLogs(lottery, userOrg);

                AddKudosLogs(lottery, refundLogs, userOrg);

                UpdateUserProfiles(lottery, refundLogs, userOrg);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                _asyncRunner.Run<ILotteryService>(n =>
                    n.UpdateRefundFailedFlag(lottery.Id, isFailed: true, userOrg), _uow.ConnectionName);
            }
        }

        private void UpdateUserProfiles(Lottery lottery, IEnumerable<AddKudosLogDTO> kudosLogs, UserAndOrganizationDTO userOrg)
        {
            if (lottery.Status == (int)LotteryStatus.RefundLogsCreated)
            {
                var userIds = kudosLogs.Select(x => x.ReceivingUserIds.First());

                _kudosService.UpdateProfilesFromUserIds(userIds, userOrg);
                _lotteryService.UpdateLotteryStatus(lottery.Id, LotteryStatus.Aborted);
                _lotteryService.UpdateRefundFailedFlag(lottery.Id, isFailed: false, userOrg);
            }
        }

        private void AddKudosLogs(Lottery lottery, IEnumerable<AddKudosLogDTO> kudosLogs, UserAndOrganizationDTO userOrg)
        {
            if (lottery.Status == (int)LotteryStatus.RefundStarted)
            {
                _kudosService.AddRefundKudosLogs(kudosLogs);
                lottery.Status = (int)LotteryStatus.RefundLogsCreated;
                _lotteryService.UpdateLotteryStatusAndSave(lottery.Id, LotteryStatus.RefundLogsCreated, userOrg);
            }
        }

        private IList<AddKudosLogDTO> CreateKudosLogs(Lottery lottery, UserAndOrganizationDTO userOrg)
        {
            var kudosTypeId = _kudosService.GetKudosTypeId(ConstBusinessLayer.KudosTypeEnum.Refund);
            var usersToRefund = _participantService.GetParticipantsCounted(lottery.Id);
            var usersToSendKudos = new List<AddKudosLogDTO>();

            foreach (var user in usersToRefund)
            {
                var totalReturn = user.Tickets * lottery.EntryFee;
                var kudosLog = new AddKudosLogDTO
                {
                    ReceivingUserIds = new List<string> { user.UserId },
                    PointsTypeId = kudosTypeId,
                    MultiplyBy = totalReturn,
                    Comment = CreateComment(lottery, totalReturn),
                    UserId = userOrg.UserId,
                    OrganizationId = userOrg.OrganizationId
                };
                usersToSendKudos.Add(kudosLog);
            }

            return usersToSendKudos;
        }

        private static string CreateComment(Lottery lottery, int total)
        {
            return $"Refund for lottery {lottery.Title}. Returned {total} kudos.";
        }
    }
}