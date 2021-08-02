using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Domain.Services.Kudos;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public class LotteryAbortJob : ILotteryAbortJob
    {
        private readonly ILotteryService _lotteryService;
        private readonly IParticipantService _participantService;
        private readonly IKudosService _kudosService;
        private readonly IAsyncRunner _asyncRunner;
        private readonly ILogger _logger;
        private readonly IUnitOfWork2 _uow;

        public LotteryAbortJob(IKudosService kudosService,
            IParticipantService participantService,
            ILogger logger,
            IAsyncRunner asyncRunner,
            IUnitOfWork2 uow,
            ILotteryService lotteryService)
        {
            _kudosService = kudosService;
            _participantService = participantService;
            _logger = logger;
            _asyncRunner = asyncRunner;
            _uow = uow;
            _lotteryService = lotteryService;
        }

        public async Task RefundLotteryAsync(int lotteryId, UserAndOrganizationDTO userOrg)
        {
            var lottery = await _lotteryService.GetLotteryAsync(lotteryId);
            if (lottery == null || lottery.OrganizationId != userOrg.OrganizationId)
            {
                return;
            }

            try
            {
                var refundLogs = await CreateKudosLogsAsync(lottery, userOrg);
                await AddKudosLogs(lottery, refundLogs, userOrg);
                await UpdateUserProfiles(lottery, refundLogs, userOrg);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                _asyncRunner.Run<ILotteryService>(async n => await n.UpdateRefundFailedFlagAsync(lottery.Id, true, userOrg), _uow.ConnectionName);
            }
        }

        private async Task UpdateUserProfiles(Lottery lottery, IEnumerable<AddKudosLogDTO> kudosLogs, UserAndOrganizationDTO userOrg)
        {
            if (lottery.Status != (int)LotteryStatus.RefundLogsCreated)
            {
                return;
            }

            var userIds = kudosLogs.Select(x => x.ReceivingUserIds.First());

            _kudosService.UpdateProfilesFromUserIds(userIds, userOrg);
            lottery.Status = (int)LotteryStatus.Refunded;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private async Task AddKudosLogs(Lottery lottery, IEnumerable<AddKudosLogDTO> kudosLogs, UserAndOrganizationDTO userOrg)
        {
            if (lottery.Status != (int)LotteryStatus.RefundStarted)
            {
                return;
            }

            _kudosService.AddRefundKudosLogs(kudosLogs);
            lottery.Status = (int)LotteryStatus.RefundLogsCreated;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private async Task<IList<AddKudosLogDTO>> CreateKudosLogsAsync(Lottery lottery, UserAndOrganizationDTO userOrg)
        {
            var kudosTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Refund);
            var usersToRefund = await _participantService.GetParticipantsCountedAsync(lottery.Id);
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