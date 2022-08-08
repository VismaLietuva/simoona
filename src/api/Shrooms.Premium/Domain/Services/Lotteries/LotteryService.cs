using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

        public async Task<LotteryDto> CreateLotteryAsync(LotteryDto newLotteryDto, UserAndOrganizationDto userOrg)
        {
            var newLottery = MapLotteryDtoToLottery(newLotteryDto, userOrg);

            _lotteriesDbSet.Add(newLottery);

            await _uow.SaveChangesAsync(userOrg.UserId);
            
            newLotteryDto.Id = newLottery.Id;

            NotifyAboutStartedLottery(newLottery, userOrg);

            return newLotteryDto;
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

        public async Task<IEnumerable<LotteryDetailsDto>> GetFilteredLotteriesAsync(string filter, UserAndOrganizationDto userOrg)
        {
            return await _lotteriesDbSet
                .Where(x => x.OrganizationId == userOrg.OrganizationId)
                .Where(x => filter == null || x.Title.Contains(filter))
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(x => x.RefundFailed)
                .ThenByDescending(x => x.Status == LotteryStatus.Started || x.Status == LotteryStatus.Drafted)
                .ThenByDescending(lottery => lottery.EndDate)
                .ToListAsync();
        }

        public async Task<IPagedList<LotteryDetailsDto>> GetPagedLotteriesAsync(LotteryListingArgsDto args, UserAndOrganizationDto userOrg)
        {
            var filteredLotteries = await GetFilteredLotteriesAsync(args.Filter, userOrg);

            return await filteredLotteries.ToPagedListAsync(args.Page, args.PageSize);
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

        public async Task BuyLotteryTicketsAsync(BuyLotteryTicketsDto buyLotteryTicketsDto, UserAndOrganizationDto userOrg)
        {
            var buyerApplicationUser = await _userService.GetApplicationUserAsync(userOrg.UserId);

            _lotteryValidator.CheckIfBuyerExists(buyerApplicationUser);

            var lotteryDetails = await GetLotteryDetailsAsync(buyLotteryTicketsDto.LotteryId, true, userOrg);

            _lotteryValidator.CheckIfLotteryExists(lotteryDetails);
            _lotteryValidator.CheckIfLotteryEnded(lotteryDetails);

            if (!CanGiftTickets(lotteryDetails))
            {
                _lotteryValidator.CheckIfGiftedTicketsReceiversExist(buyLotteryTicketsDto);
            }

            var isGiftingTickets = buyLotteryTicketsDto.ReceivingUserIds.Any();

            if (CanGiftTickets(lotteryDetails) && isGiftingTickets)
            {
                _lotteryValidator.CheckIfGiftedTicketLimitIsExceeded(lotteryDetails.Buyer, buyLotteryTicketsDto);

                await _lotteryValidator.CheckIfGiftReceiversExistAsync(buyLotteryTicketsDto, userOrg);
            }

            if (!_lotteryValidator.IsValidTicketCount(buyLotteryTicketsDto))
            {
                await AddKudosLogForCheatingAsync(userOrg, lotteryDetails.EntryFee, buyerApplicationUser);

                throw new LotteryException("Thanks for trying - you were charged double Kudos for this without getting a ticket.");
            }

            var ticketReceiverCount = isGiftingTickets ? buyLotteryTicketsDto.ReceivingUserIds.Count() : 1;

            var totalTicketCost = lotteryDetails.EntryFee * buyLotteryTicketsDto.Tickets * ticketReceiverCount;

            _lotteryValidator.CheckIfUserHasEnoughKudos(buyerApplicationUser, totalTicketCost);

            var totalBoughtTicketCount = buyLotteryTicketsDto.Tickets * ticketReceiverCount;

            var kudosLog = new AddKudosLogDto
            {
                ReceivingUserIds = new List<string> { userOrg.UserId },
                PointsTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Minus),
                MultiplyBy = totalTicketCost,
                Comment = $"{totalBoughtTicketCount} ticket(s) for lottery {lotteryDetails.Title}",
                UserId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId
            };

            await _kudosService.AddLotteryKudosLogAsync(kudosLog, userOrg);

            var ticketOwnerUserIds = isGiftingTickets ? buyLotteryTicketsDto.ReceivingUserIds : new string[] { userOrg.UserId };

            foreach (var ticketOwnerId in ticketOwnerUserIds)
            {
                AddLotteryTicketsForUser(buyLotteryTicketsDto.LotteryId, ticketOwnerId, userOrg.UserId, buyLotteryTicketsDto.Tickets);
            }

            await _uow.SaveChangesAsync(buyerApplicationUser.Id);
            await _kudosService.UpdateProfileKudosAsync(buyerApplicationUser, userOrg);
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

        private bool CanGiftTickets(LotteryDetailsDto detailsDto)
        {
            return detailsDto.GiftedTicketLimit > 0;
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
                Images = lotteryDto.Images,
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
            lottery.Images = draftedLotteryDto.Images;
            lottery.Modified = _systemClock.UtcNow;
            lottery.ModifiedBy = userOrg.UserId;
            lottery.GiftedTicketLimit = draftedLotteryDto.GiftedTicketLimit;
        }
    }
}
