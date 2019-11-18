using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
        private readonly IAsyncRunner _asyncRunner;
        private readonly ILogger _logger;
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Lottery> _lotteriesDbSet;

        public LotteryAbortJob(
            IKudosService kudosService,
            IParticipantService participantService,
            ILogger logger,
            IAsyncRunner asyncRunner,
            IUnitOfWork2 uow)
        {
            _kudosService = kudosService;
            _participantService = participantService;
            _logger = logger;
            _asyncRunner = asyncRunner;
            _uow = uow;
            _lotteriesDbSet = _uow.GetDbSet<Lottery>();
        }

        public void RefundLottery(int lotteryId, UserAndOrganizationDTO userOrg)
        {
            var lottery = _lotteriesDbSet.Find(lotteryId);
            if (lottery is null ||
                lottery.OrganizationId != userOrg.OrganizationId)
            {
                return;
            }

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
                    n.UpdateRefundFailedFlag(lottery.Id, true, userOrg), _uow.ConnectionName);
            }
        }

        private void UpdateUserProfiles(Lottery lottery, IEnumerable<AddKudosLogDTO> kudosLogs, UserAndOrganizationDTO userOrg)
        {
            if (lottery.Status == (int)LotteryStatus.RefundLogsCreated)
            {
                var userIds = kudosLogs.Select(x => x.ReceivingUserIds.First());

                _kudosService.UpdateProfilesFromUserIds(userIds, userOrg);
                lottery.Status = (int)LotteryStatus.Refunded;

                _uow.SaveChanges(userOrg.UserId);
            }
        }

        private void AddKudosLogs(Lottery lottery, IEnumerable<AddKudosLogDTO> kudosLogs, UserAndOrganizationDTO userOrg)
        {
            if (lottery.Status == (int)LotteryStatus.RefundStarted)
            {
                _kudosService.AddRefundKudosLogs(kudosLogs);
                lottery.Status = (int)LotteryStatus.RefundLogsCreated;

                _uow.SaveChanges(userOrg.UserId);
            }
        }

        private IList<AddKudosLogDTO> CreateKudosLogs(Lottery lottery, UserAndOrganizationDTO userOrg)
        {
            var kudosTypeId = _kudosService.GetKudosTypeId(KudosTypeEnum.Refund);
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