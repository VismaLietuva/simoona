using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.UserService;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using Shrooms.Premium.Domain.Services.Args;
using Shrooms.Premium.Domain.Services.Email.Lotteries;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public class LotteryService : ILotteryService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<Lottery> _lotteriesDbSet;
        private readonly DbSet<LotteryParticipant> _participantsDbSet;
        private readonly IAsyncRunner _asyncRunner;
        private readonly IMapper _mapper;
        private readonly IParticipantService _participantService;
        private readonly IUserService _userService;
        private readonly IKudosService _kudosService;
        private readonly ISystemClock _systemClock;
        
        public LotteryService(IUnitOfWork2 uow,
            IMapper mapper,
            IParticipantService participantService,
            IUserService userService,
            IKudosService kudosService,
            IAsyncRunner asyncRunner,
            ISystemClock systemClock)
        {
            _uow = uow;
            _lotteriesDbSet = uow.GetDbSet<Lottery>();
            _asyncRunner = asyncRunner;
            _mapper = mapper;
            _participantsDbSet = uow.GetDbSet<LotteryParticipant>();
            _participantService = participantService;
            _userService = userService;
            _kudosService = kudosService;
            _systemClock = systemClock;
        }

        public async Task<Lottery> GetLotteryAsync(int lotteryId)
        {
            return await _lotteriesDbSet.FindAsync(lotteryId);
        }

        public async Task<LotteryDto> CreateLotteryAsync(LotteryDto newLotteryDto, UserAndOrganizationDto userOrg)
        {
            if (newLotteryDto.EndDate < _systemClock.UtcNow)
            {
                throw new LotteryException("Lottery can't start in the past.");
            }

            if (newLotteryDto.EntryFee < 1)
            {
                throw new LotteryException("Invalid entry fee.");
            }

            if (newLotteryDto.Status != (int)LotteryStatus.Started &&
                newLotteryDto.Status != (int)LotteryStatus.Drafted)
            {
                throw new LotteryException("Invalid status of created lottery.");
            }

            var newLottery = MapNewLottery(newLotteryDto, userOrg);
            _lotteriesDbSet.Add(newLottery);

            await _uow.SaveChangesAsync(userOrg.UserId);

            NotifyAboutStartedLottery(newLottery, userOrg);

            newLotteryDto.Id = newLottery.Id;

            return newLotteryDto;
        }

        public async Task EditDraftedLotteryAsync(LotteryDto lotteryDto, UserAndOrganizationDto userOrg)
        {
            if (lotteryDto.EndDate < _systemClock.UtcNow)
            {
                throw new LotteryException("Lottery can't start in the past.");
            }

            var lottery = await _lotteriesDbSet.FindAsync(lotteryDto.Id);

            if (lottery != null && lottery.Status != (int)LotteryStatus.Drafted)
            {
                throw new LotteryException("Editing is forbidden for not drafted lottery.");
            }

            UpdateDraftedLottery(lottery, lotteryDto, userOrg);

            await _uow.SaveChangesAsync(false);

            NotifyAboutStartedLottery(lottery, userOrg);
        }

        public async Task EditStartedLotteryAsync(EditStartedLotteryDto lotteryDto)
        {
            var lottery = await _lotteriesDbSet.FindAsync(lotteryDto.Id);

            if (lottery != null && lottery.Status != (int)LotteryStatus.Started)
            {
                throw new LotteryException("Lottery is not running.");
            }

            if (lottery != null)
            {
                lottery.Description = lotteryDto.Description;
                await _uow.SaveChangesAsync(false);
            }
        }

        public async Task<LotteryDetailsDto> GetLotteryDetailsAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await _lotteriesDbSet
                .FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return null;
            }

            var lotteryDetailsDto = _mapper.Map<Lottery, LotteryDetailsDto>(lottery);
            lotteryDetailsDto.Participants = await _participantsDbSet.CountAsync(p => p.LotteryId == lotteryId);

            return lotteryDetailsDto;
        }

        public async Task<bool> AbortLotteryAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await _lotteriesDbSet
                .FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return false;
            }

            if (lottery.Status == (int)LotteryStatus.Started || lottery.Status == (int)LotteryStatus.Expired)
            {
                lottery.Status = (int)LotteryStatus.RefundStarted;
                await _uow.SaveChangesAsync();

                _asyncRunner.Run<ILotteryAbortJob>(async notifier => await notifier.RefundLotteryAsync(lottery.Id, userOrg), _uow.ConnectionName);
            }
            else if (lottery.Status == (int)LotteryStatus.Drafted)
            {
                lottery.Status = (int)LotteryStatus.Deleted;
                await _uow.SaveChangesAsync();
            }

            return lottery.Status == (int)LotteryStatus.Deleted || lottery.Status == (int)LotteryStatus.RefundStarted;
        }

        public async Task RefundParticipantsAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await _lotteriesDbSet
                .FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return;
            }

            if (!lottery.IsRefundFailed)
            {
                return;
            }

            lottery.IsRefundFailed = false;
            await _uow.SaveChangesAsync(userOrg.UserId);

            _asyncRunner.Run<ILotteryAbortJob>(async notifier => await notifier.RefundLotteryAsync(lottery.Id, userOrg), _uow.ConnectionName);
        }

        public async Task UpdateRefundFailedFlagAsync(int lotteryId, bool isFailed, UserAndOrganizationDto userOrg)
        {
            var lottery = await _lotteriesDbSet.FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return;
            }

            lottery.IsRefundFailed = isFailed;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task FinishLotteryAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await _lotteriesDbSet.FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return;
            }

            lottery.Status = (int)LotteryStatus.Ended;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<LotteryStatsDto> GetLotteryStatsAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await _lotteriesDbSet
                .FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return null;
            }

            var participants = await _participantService.GetParticipantsCountedAsync(lotteryId);

            var ticketsSold = participants.Sum(participant => participant.Tickets);

            return new LotteryStatsDto
            {
                TotalParticipants = participants.Count,
                TicketsSold = ticketsSold,
                KudosSpent = ticketsSold * lottery.EntryFee
            };
        }

        public async Task<IEnumerable<LotteryDetailsDto>> GetLotteriesAsync(UserAndOrganizationDto userOrganization)
        {
            return await _lotteriesDbSet
                .Where(p => p.OrganizationId == userOrganization.OrganizationId)
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(_byEndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LotteryDetailsDto>> GetFilteredLotteriesAsync(string filter, UserAndOrganizationDto userOrg)
        {
            return await _lotteriesDbSet
                .Where(x => x.OrganizationId == userOrg.OrganizationId)
                .Where(x => x.Title.Contains(filter))
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(x => x.RefundFailed)
                .ThenByDescending(x => x.Status == (int)LotteryStatus.Started || x.Status == (int)LotteryStatus.Drafted)
                .ThenByDescending(_byEndDate)
                .ToListAsync();
        }

        public async Task<IPagedList<LotteryDetailsDto>> GetPagedLotteriesAsync(GetPagedLotteriesArgs args)
        {
            var filteredLotteries = await GetFilteredLotteriesAsync(args.Filter, args.UserOrg);
            return await filteredLotteries.ToPagedListAsync(args.PageNumber, args.PageSize);
        }

        public async Task<LotteryStatusDto> GetLotteryStatusAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await _lotteriesDbSet
                .FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return null;
            }

            return new LotteryStatusDto
            {
                LotteryStatus = lottery.Status,
                RefundFailed = lottery.IsRefundFailed
            };
        }

        public async Task BuyLotteryTicketAsync(BuyLotteryTicketDto lotteryTicketDto, UserAndOrganizationDto userOrg)
        {
            var applicationUser = await _userService.GetApplicationUserAsync(userOrg.UserId);

            var lotteryDetails = await GetLotteryDetailsAsync(lotteryTicketDto.LotteryId, userOrg);

            if (lotteryDetails is null || applicationUser is null)
            {
                return;
            }

            if (lotteryTicketDto.Tickets <= 0)
            {
                await AddKudosLogForCheatingAsync(userOrg, lotteryDetails.EntryFee, applicationUser);
                throw new LotteryException("Thanks for trying - you were charged double Kudos for this without getting a ticket.");
            }

            if (applicationUser.RemainingKudos < lotteryDetails.EntryFee * lotteryTicketDto.Tickets)
            {
                throw new LotteryException("User does not have enough kudos for the purchase.");
            }

            if (_systemClock.UtcNow > lotteryDetails.EndDate)
            {
                throw new LotteryException("Lottery has already ended.");
            }

            var kudosLog = new AddKudosLogDto
            {
                ReceivingUserIds = new List<string> { userOrg.UserId },
                PointsTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Minus),
                MultiplyBy = lotteryTicketDto.Tickets * lotteryDetails.EntryFee,
                Comment = $"{lotteryTicketDto.Tickets} ticket(s) for lottery {lotteryDetails.Title}",
                UserId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId
            };

            await _kudosService.AddLotteryKudosLogAsync(kudosLog, userOrg);

            if (applicationUser.RemainingKudos < 0)
            {
                kudosLog.PointsTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Refund);
                await _kudosService.AddRefundKudosLogsAsync(new List<AddKudosLogDto> { kudosLog });
            }
            else
            {
                for (var i = 0; i < lotteryTicketDto.Tickets; i++)
                {
                    _participantsDbSet.Add(MapNewLotteryParticipant(lotteryTicketDto, userOrg));
                }
            }

            await _uow.SaveChangesAsync(applicationUser.Id);
            await _kudosService.UpdateProfileKudosAsync(applicationUser, userOrg);
        }

        public async Task<List<LotteryDetailsDto>> GetRunningLotteriesAsync(UserAndOrganizationDto userAndOrganization)
        {
            return await _lotteriesDbSet.Where(p =>
                    p.OrganizationId == userAndOrganization.OrganizationId &&
                    p.Status == (int)LotteryStatus.Started &&
                    p.EndDate > _systemClock.UtcNow)
                .Select(MapLotteriesToListItemDto)
                .OrderBy(_byEndDate)
                .ToListAsync();
        }

        private async Task AddKudosLogForCheatingAsync(UserAndOrganizationDto userOrg, int entryFee, ApplicationUser applicationUser)
        {
            var kudosLog = new AddKudosLogDto
            {
                ReceivingUserIds = new List<string> { userOrg.UserId },
                PointsTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Minus),
                MultiplyBy = entryFee * 2,
                Comment = "Cheating",
                UserId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId
            };

            await _kudosService.AddLotteryKudosLogAsync(kudosLog, userOrg);
            await _uow.SaveChangesAsync(applicationUser.Id);
            await _kudosService.UpdateProfileKudosAsync(applicationUser, userOrg);
        }

        private void NotifyAboutStartedLottery(Lottery lottery, UserAndOrganizationDto userOrg)
        {
            if (lottery.Status != (int)LotteryStatus.Started)
            {
                return;
            }

            var lotteryStartedEmailDto = new LotteryStartedEmailDto
            {
                Id = lottery.Id,
                Title = lottery.Title,
                EndDate = lottery.EndDate,
                EntryFee = lottery.EntryFee,
                Description = lottery.Description
            };

            _asyncRunner.Run<ILotteryNotificationService>(async notifier => await notifier.NotifyUsersAboutStartedLotteryAsync(lotteryStartedEmailDto, userOrg.OrganizationId),
                _uow.ConnectionName);
        }

        private readonly Expression<Func<LotteryDetailsDto, DateTime>> _byEndDate = e => e.EndDate;

        private static Expression<Func<Lottery, LotteryDetailsDto>> MapLotteriesToListItemDto =>
            e => new LotteryDetailsDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                EntryFee = e.EntryFee,
                EndDate = e.EndDate,
                Status = e.Status,
                RefundFailed = e.IsRefundFailed
            };

        private Lottery MapNewLottery(LotteryDto newLotteryDto, UserAndOrganizationDto userOrg)
        {
            var newLottery = _mapper.Map<LotteryDto, Lottery>(newLotteryDto);

            newLottery.CreatedBy = userOrg.UserId;
            newLottery.Created = _systemClock.UtcNow;
            newLottery.Modified = _systemClock.UtcNow;
            newLottery.ModifiedBy = userOrg.UserId;
            newLottery.IsRefundFailed = false;
            newLottery.OrganizationId = userOrg.OrganizationId;

            return newLottery;
        }

        private static void UpdateDraftedLottery(Lottery lottery, LotteryDto draftedLotteryDto, UserAndOrganizationDto userOrg)
        {
            lottery.EntryFee = draftedLotteryDto.EntryFee;
            lottery.EndDate = draftedLotteryDto.EndDate;
            lottery.Description = draftedLotteryDto.Description;
            lottery.Status = draftedLotteryDto.Status;
            lottery.Title = draftedLotteryDto.Title;
            lottery.Images = draftedLotteryDto.Images;
            lottery.Modified = DateTime.UtcNow;
            lottery.ModifiedBy = userOrg.UserId;
        }

        private LotteryParticipant MapNewLotteryParticipant(BuyLotteryTicketDto lotteryTicketDto, UserAndOrganizationDto userOrg)
        {
            return new LotteryParticipant
            {
                LotteryId = lotteryTicketDto.LotteryId,
                UserId = userOrg.UserId,
                Joined = _systemClock.UtcNow,
                CreatedBy = userOrg.UserId,
                ModifiedBy = userOrg.UserId,
                Modified = _systemClock.UtcNow,
                Created = _systemClock.UtcNow
            };
        }
    }
}
