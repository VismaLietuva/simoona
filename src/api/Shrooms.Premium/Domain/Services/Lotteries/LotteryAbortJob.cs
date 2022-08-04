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

        public async Task RefundLotteryAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await _lotteryService.GetLotteryByIdAsync(lotteryId, userOrg);

            if (lottery == null || lottery.OrganizationId != userOrg.OrganizationId)
            {
                return;
            }

            try
            {
                var refundLogs = await CreateKudosLogsAsync(lottery, userOrg);
                await AddKudosLogsAsync(lottery, refundLogs, userOrg);
                await UpdateUserProfilesAsync(lottery, refundLogs, userOrg);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                _asyncRunner.Run<ILotteryService>(async n => await n.UpdateRefundFailedFlagAsync(lottery.Id, true, userOrg), _uow.ConnectionName);
            }
        }

        private async Task UpdateUserProfilesAsync(Lottery lottery, IEnumerable<AddKudosLogDto> kudosLogs, UserAndOrganizationDto userOrg)
        {
            if (lottery.Status != LotteryStatus.RefundLogsCreated)
            {
                return;
            }

            var userIds = kudosLogs.Select(x => x.ReceivingUserIds.First());

            await _kudosService.UpdateProfilesFromUserIdsAsync(userIds, userOrg);
            lottery.Status = LotteryStatus.Refunded;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private async Task AddKudosLogsAsync(Lottery lottery, IEnumerable<AddKudosLogDto> kudosLogs, UserAndOrganizationDto userOrg)
        {
            if (lottery.Status != LotteryStatus.RefundStarted)
            {
                return;
            }

            await _kudosService.AddRefundKudosLogsAsync(kudosLogs);
            lottery.Status = LotteryStatus.RefundLogsCreated;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private async Task<IList<AddKudosLogDto>> CreateKudosLogsAsync(Lottery lottery, UserAndOrganizationDto userOrg)
        {
            var kudosTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Refund);
            var usersToRefund = await _participantService.GetParticipantsCountedAsync(lottery.Id);
            var usersToSendKudos = new List<AddKudosLogDto>();

            foreach (var user in usersToRefund)
            {
                var totalReturn = user.Tickets * lottery.EntryFee;
                var kudosLog = new AddKudosLogDto
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