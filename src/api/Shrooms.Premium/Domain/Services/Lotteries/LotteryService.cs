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
using Shrooms.Premium.Domain.DomainServiceValidators.Lotteries;
using Shrooms.Premium.Domain.Services.Email.Lotteries;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Domain.Extensions;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public class LotteryService : ILotteryService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IAsyncRunner _asyncRunner;
        private readonly ILotteryParticipantService _participantService;
        private readonly IUserService _userService;
        private readonly IKudosService _kudosService;
        private readonly ISystemClock _systemClock;
        private readonly ILotteryValidator _lotteryValidator;

        private readonly DbSet<Lottery> _lotteriesDbSet;
        private readonly DbSet<LotteryParticipant> _participantsDbSet;

        public LotteryService(IUnitOfWork2 uow,
            ILotteryParticipantService participantService,
            IUserService userService,
            IKudosService kudosService,
            IAsyncRunner asyncRunner,
            ISystemClock systemClock,
            ILotteryValidator lotteryValidator)
        {
            _uow = uow;
            _asyncRunner = asyncRunner;
            _participantService = participantService;
            _userService = userService;
            _kudosService = kudosService;
            _systemClock = systemClock;
            _lotteryValidator = lotteryValidator;

            _lotteriesDbSet = uow.GetDbSet<Lottery>();
            _participantsDbSet = uow.GetDbSet<LotteryParticipant>();
        }

        public async Task CreateLotteryAsync(LotteryDto newLotteryDto, UserAndOrganizationDto userOrg)
        {
            var newLottery = MapLotteryDtoToLottery(newLotteryDto, userOrg);

            _lotteriesDbSet.Add(newLottery);

            await _uow.SaveChangesAsync(userOrg.UserId);

            newLotteryDto.Id = newLottery.Id;

            NotifyAboutStartedLottery(newLottery, userOrg);
        }

        public async Task EditDraftedLotteryAsync(LotteryDto lotteryDto, UserAndOrganizationDto userOrg)
        {
            var lottery = await GetLotteryByIdAsync(lotteryDto.Id, userOrg);

            _lotteryValidator.CheckIfLotteryExists(lottery);
            _lotteryValidator.CheckIfLotteryIsDrafted(lottery);

            UpdateDraftedLottery(lottery, lotteryDto, userOrg);

            await _uow.SaveChangesAsync(false);

            NotifyAboutStartedLottery(lottery, userOrg);
        }

        public async Task EditStartedLotteryAsync(EditStartedLotteryDto lotteryDto, UserAndOrganizationDto userOrg)
        {
            var lottery = await GetLotteryByIdAsync(lotteryDto.Id, userOrg);

            _lotteryValidator.CheckIfLotteryExists(lottery);
            _lotteryValidator.CheckIfLotteryIsStarted(lottery);

            lottery.Description = lotteryDto.Description;

            await _uow.SaveChangesAsync(false);
        }

        public async Task<LotteryDetailsDto> GetLotteryDetailsAsync(int lotteryId, bool includeBuyer, UserAndOrganizationDto userOrg)
        {
            var lottery = await GetLotteryByIdAsync(lotteryId, userOrg);

            if (lottery is null)
            {
                return null;
            }

            var detailsDto = MapLotteryToLotteryDetailsDto(lottery);

            var allLotteryParticipants = await _participantsDbSet
                .Include(participant => participant.Lottery)
                .Where(participant => participant.LotteryId == lotteryId &&
                                      participant.Lottery.OrganizationId == userOrg.OrganizationId)
                .ToListAsync();


            detailsDto.Participants = allLotteryParticipants.Count;

            if (includeBuyer)
            {
                var buyerApplicationUser = await _userService.GetApplicationUserAsync(userOrg.UserId);

                _lotteryValidator.CheckIfBuyerExists(buyerApplicationUser);

                var previouslyGiftedTicketCount = allLotteryParticipants
                    .Where(participant => participant.CreatedBy == userOrg.UserId &&
                                          participant.UserId != userOrg.UserId)
                    .Count();

                detailsDto.Buyer = new LotteryDetailsBuyerDto
                {
                    RemainingGiftedTicketCount = detailsDto.GiftedTicketLimit - previouslyGiftedTicketCount,
                    RemainingKudos = buyerApplicationUser.RemainingKudos
                };
            }

            return detailsDto;
        }

        public async Task<bool> AbortLotteryAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await GetLotteryByIdAsync(lotteryId, userOrg);

            if (lottery is null)
            {
                return false;
            }

            if (lottery.Status == LotteryStatus.Started || lottery.Status == LotteryStatus.Expired)
            {
                lottery.Status = LotteryStatus.RefundStarted;

                await _uow.SaveChangesAsync();

                _asyncRunner.Run<ILotteryAbortJob>(async notifier => await notifier.RefundLotteryAsync(lottery.Id, userOrg), _uow.ConnectionName);
            }
            else if (lottery.Status == LotteryStatus.Drafted)
            {
                lottery.Status = LotteryStatus.Deleted;

                await _uow.SaveChangesAsync();
            }

            return lottery.Status == LotteryStatus.Deleted || lottery.Status == LotteryStatus.RefundStarted;
        }

        public async Task RefundParticipantsAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await GetLotteryByIdAsync(lotteryId, userOrg);

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
            var lottery = await GetLotteryByIdAsync(lotteryId, userOrg);

            if (lottery is null)
            {
                return;
            }

            lottery.IsRefundFailed = isFailed;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task FinishLotteryAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await GetLotteryByIdAsync(lotteryId, userOrg);

            if (lottery is null)
            {
                return;
            }

            lottery.Status = LotteryStatus.Ended;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<LotteryStatsDto> GetLotteryStatsAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await GetLotteryByIdAsync(lotteryId, userOrg);

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
                .OrderByDescending(lottery => lottery.EndDate)
                .ToListAsync();
        }

        public async Task<IPagedList<LotteryDetailsDto>> GetPagedLotteriesAsync(LotteryListingArgsDto args, UserAndOrganizationDto userOrg)
        {
            var sortable = args.AddSortablePropertiesToStart((nameof(LotteryDetailsDto.RefundFailed), SortDirection.Descending))
                .AddSortablePropertiesToEnd((nameof(LotteryDetailsDto.Id), SortDirection.Descending));

            return await _lotteriesDbSet
                .Where(lottery => lottery.OrganizationId == userOrg.OrganizationId &&
                                  (args.Filter == null || lottery.Title.Contains(args.Filter)))
                .Select(MapLotteriesToListItemDto)
                .OrderByPropertyNames(sortable)
                .ToPagedListAsync(args.Page, args.PageSize);
        }

        public async Task<LotteryStatusDto> GetLotteryStatusAsync(int lotteryId, UserAndOrganizationDto userOrg)
        {
            var lottery = await GetLotteryByIdAsync(lotteryId, userOrg);

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

        public async Task BuyLotteryTicketsAsync(BuyLotteryTicketsDto buyTicketsDto, UserAndOrganizationDto userOrg)
        {
            var buyerUser = await _userService.GetApplicationUserAsync(userOrg.UserId);

            _lotteryValidator.CheckIfBuyerExists(buyerUser);

            var isGiftingTickets = buyTicketsDto.Receivers.Any();

            var lotteryDetailsDto = await GetLotteryDetailsAsync(buyTicketsDto.LotteryId, isGiftingTickets, userOrg);

            _lotteryValidator.CheckIfLotteryExists(lotteryDetailsDto);
            _lotteryValidator.CheckIfLotteryEnded(lotteryDetailsDto);

            if (isGiftingTickets)
            {
                await GiftLotteryTicketsAsync(buyerUser, lotteryDetailsDto, buyTicketsDto, userOrg);
            }
            else
            {
                await BuyLotteryTicketsAsync(buyerUser, lotteryDetailsDto, buyTicketsDto, userOrg);
            }

            await _uow.SaveChangesAsync(buyerUser.Id);
            await _kudosService.UpdateProfileKudosAsync(buyerUser, userOrg);
        }

        public async Task<List<LotteryDetailsDto>> GetRunningLotteriesAsync(UserAndOrganizationDto userAndOrganization)
        {
            return await _lotteriesDbSet
                .Where(p => p.OrganizationId == userAndOrganization.OrganizationId &&
                            p.Status == LotteryStatus.Started &&
                            p.EndDate > _systemClock.UtcNow)
                .Select(MapLotteriesToListItemDto)
                .OrderBy(lottery => lottery.EndDate)
                .ToListAsync();
        }

        public async Task<Lottery> GetLotteryByIdAsync(int id, UserAndOrganizationDto userOrg)
        {
            return await _lotteriesDbSet.FirstOrDefaultAsync(FindLotteryById(id, userOrg));
        }

        private async Task BuyLotteryTicketsAsync(
           ApplicationUser buyerUser,
           LotteryDetailsDto lotteryDetailsDto,
           BuyLotteryTicketsDto buyTicketsDto,
           UserAndOrganizationDto userOrg)
        {
            if (!_lotteryValidator.IsValidTicketCount(buyTicketsDto))
            {
                await AddKudosLogForCheatingAsync(userOrg, lotteryDetailsDto.EntryFee, buyerUser);

                throw new LotteryException("Thanks for trying - you were charged double Kudos for this without getting a ticket.");
            }

            var totalTicketCost = lotteryDetailsDto.EntryFee * buyTicketsDto.TicketCount;

            _lotteryValidator.CheckIfUserHasEnoughKudos(buyerUser, totalTicketCost);

            var minusKudosTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Minus);

            var kudosLog = new AddKudosLogDto
            {
                ReceivingUserIds = new List<string> { buyerUser.Id },
                PointsTypeId = minusKudosTypeId,
                MultiplyBy = totalTicketCost,
                Comment = $"{buyTicketsDto.TicketCount} ticket(s) for lottery {lotteryDetailsDto.Title}",
                UserId = buyerUser.Id,
                OrganizationId = buyerUser.OrganizationId
            };

            await _kudosService.AddLotteryKudosLogAsync(kudosLog, userOrg);

            AddLotteryTicketsForUser(lotteryDetailsDto.Id, buyerUser.Id, buyerUser.Id, buyTicketsDto.TicketCount);
        }

        private async Task GiftLotteryTicketsAsync(
            ApplicationUser buyerUser,
            LotteryDetailsDto lotteryDetailsDto,
            BuyLotteryTicketsDto buyTicketsDto,
            UserAndOrganizationDto userOrg)
        {
            _lotteryValidator.CheckIfLotteryAllowsGifting(lotteryDetailsDto);

            var receivers = buyTicketsDto.Receivers;
            var receiverIds = receivers.Select(receiver => receiver.UserId);

            var totalTicketCount = receivers.Sum(receiver => receiver.TicketCount);

            _lotteryValidator.CheckIfGiftedTicketLimitIsExceeded(lotteryDetailsDto.Buyer, totalTicketCount);
            await _lotteryValidator.CheckIfGiftReceiversExistAsync(receiverIds, userOrg);

            var totalTicketCost = lotteryDetailsDto.EntryFee * totalTicketCount;

            _lotteryValidator.CheckIfUserHasEnoughKudos(buyerUser, totalTicketCost);

            var kudosMinusTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Minus);

            var kudosLog = new AddKudosLogDto
            {
                ReceivingUserIds = receiverIds,
                PointsTypeId = kudosMinusTypeId,
                MultiplyBy = totalTicketCost,
                Comment = $"{buyTicketsDto.TicketCount} ticket(s) for lottery {lotteryDetailsDto.Title}",
                UserId = buyerUser.Id,
                OrganizationId = buyerUser.OrganizationId
            };

            await _kudosService.AddLotteryKudosLogAsync(kudosLog, userOrg);

            foreach (var receiver in receivers)
            {
                AddLotteryTicketsForUser(lotteryDetailsDto.Id, receiver.UserId, buyerUser.Id, receiver.TicketCount);
            }
        }

        private void AddLotteryTicketsForUser(int lotteryId, string userId, string buyerUserId, int ticketCount)
        {
            for (var i = 0; i < ticketCount; i++)
            {
                _participantsDbSet.Add(new LotteryParticipant
                {
                    LotteryId = lotteryId,
                    UserId = userId,
                    Joined = _systemClock.UtcNow,
                    CreatedBy = buyerUserId,
                    ModifiedBy = buyerUserId,
                    Modified = _systemClock.UtcNow,
                    Created = _systemClock.UtcNow,
                });
            }
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
        }

        private void NotifyAboutStartedLottery(Lottery lottery, UserAndOrganizationDto userOrg)
        {
            if (lottery.Status != LotteryStatus.Started)
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

        private static Expression<Func<Lottery, bool>> FindLotteryById(int id, UserAndOrganizationDto userOrg)
        {
            return lottery => lottery.Id == id && lottery.OrganizationId == userOrg.OrganizationId;
        }

        private Lottery MapLotteryDtoToLottery(LotteryDto lotteryDto, UserAndOrganizationDto userOrg)
        {
            return new Lottery
            {
                Id = lotteryDto.Id,
                Status = lotteryDto.Status,
                Title = lotteryDto.Title,
                Description = lotteryDto.Description,
                EndDate = lotteryDto.EndDate,
                EntryFee = lotteryDto.EntryFee,
                Images = new ImageCollection(lotteryDto.Images),
                GiftedTicketLimit = lotteryDto.GiftedTicketLimit,
                Created = _systemClock.UtcNow,
                CreatedBy = userOrg.UserId,
                Modified = _systemClock.UtcNow,
                ModifiedBy = userOrg.UserId,
                IsRefundFailed = false,
                OrganizationId = userOrg.OrganizationId
            };
        }

        private LotteryDetailsDto MapLotteryToLotteryDetailsDto(Lottery lottery)
        {
            return new LotteryDetailsDto
            {
                Id = lottery.Id,
                Title = lottery.Title,
                Description = lottery.Description,
                EndDate = lottery.EndDate,
                Status = lottery.Status,
                EntryFee = lottery.EntryFee,
                Images = lottery.Images,
                RefundFailed = lottery.IsRefundFailed,
                GiftedTicketLimit = lottery.GiftedTicketLimit
            };
        }

        private static Expression<Func<Lottery, LotteryDetailsDto>> MapLotteriesToListItemDto =>
            e => new LotteryDetailsDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                EntryFee = e.EntryFee,
                EndDate = e.EndDate,
                Status = e.Status,
                RefundFailed = e.IsRefundFailed,
                GiftedTicketLimit = e.GiftedTicketLimit,
            };

        private void UpdateDraftedLottery(Lottery lottery, LotteryDto draftedLotteryDto, UserAndOrganizationDto userOrg)
        {
            lottery.EntryFee = draftedLotteryDto.EntryFee;
            lottery.EndDate = draftedLotteryDto.EndDate;
            lottery.Description = draftedLotteryDto.Description;
            lottery.Status = draftedLotteryDto.Status;
            lottery.Title = draftedLotteryDto.Title;
            lottery.Images = new ImageCollection(draftedLotteryDto.Images);
            lottery.Modified = _systemClock.UtcNow;
            lottery.ModifiedBy = userOrg.UserId;
            lottery.GiftedTicketLimit = draftedLotteryDto.GiftedTicketLimit;
        }
    }
}
