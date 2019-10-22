using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using PagedList;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Args;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.Infrastructure.FireAndForget;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.Domain.Services.Lotteries
{
    public class LotteryService : ILotteryService
    {
        private readonly IUnitOfWork2 _uow;

        private readonly IDbSet<Lottery> _lotteriesDbSet;

        private readonly IAsyncRunner _asyncRunner;

        private readonly IMapper _mapper;

        public LotteryService(IUnitOfWork2 uow, IMapper mapper, IAsyncRunner asyncRunner)
        {
            _uow = uow;
            _lotteriesDbSet = uow.GetDbSet<Lottery>();
            _asyncRunner = asyncRunner;
            _mapper = mapper;
        }

        public async Task<CreateLotteryDTO> CreateLottery(CreateLotteryDTO newLotteryDTO)
        {
            if (newLotteryDTO.EndDate < DateTime.UtcNow)
            {
                throw new LotteryException("Lottery can't start in the past.");
            }

            var newLottery = MapNewLottery(newLotteryDTO);
            newLottery.Status = (int)LotteryStatus.Started;

            _lotteriesDbSet.Add(newLottery);
            await _uow.SaveChangesAsync(newLotteryDTO.UserId);

            newLotteryDTO.Id = newLottery.Id;

            return newLotteryDTO;
        }

        public void EditDraftedLottery(EditDraftedLotteryDTO lotteryDTO)
        {
            var lottery = _lotteriesDbSet.Find(lotteryDTO.Id);

            if (lottery.Status != (int)LotteryStatus.Drafted)
            {
                throw new LotteryException("Lottery has started or ended");
            }

            UpdateDraftedLottery(lottery, lotteryDTO);
            _uow.SaveChanges(false);
        }

        public void EditStartedLottery(EditStartedLotteryDTO lotteryDTO)
        {
            var lottery = _lotteriesDbSet.Find(lotteryDTO.Id);

            if (lottery.Status != (int)LotteryStatus.Started)
            {
                throw new LotteryException("Lottery is not running");
            }

            lottery.Description = lotteryDTO.Description;
            _uow.SaveChanges();
        }

        public LotteryDetailsDTO GetLotteryDetails(int id, UserAndOrganizationDTO userOrg)
        {
            var lottery = _lotteriesDbSet.Find(id);

            if (lottery == null)
            {
                throw new LotteryException("Lottery not found");
            }

            return _mapper.Map<Lottery, LotteryDetailsDTO>(lottery);
        }

        public void AbortLottery(int id, UserAndOrganizationDTO userOrg)
        {
            var lottery = _lotteriesDbSet.Find(id);

            if (lottery != null)
            {
                if (lottery.Status == (int)LotteryStatus.Started)
                {
                    _asyncRunner.Run<ILotteryAbortJob>(n => n.RefundLottery(lottery, userOrg), _uow.ConnectionName);
                }

                lottery.Status = (int)LotteryStatus.RefundStarted;

                _uow.SaveChanges();
            }
        }

        public void RefundParticipants(int id, UserAndOrganizationDTO userOrg)
        {
            var lottery = _lotteriesDbSet.Find(id);

            if (lottery.Status == (int)LotteryStatus.RefundFailed)
            {
                _asyncRunner.Run<ILotteryAbortJob>(n => n.RefundLottery(lottery, userOrg), _uow.ConnectionName);
            }
        }

        public IEnumerable<LotteryDetailsDTO> GetLotteries(UserAndOrganizationDTO userOrganization)
        {
            return _lotteriesDbSet
                .Where(p => p.OrganizationId == userOrganization.OrganizationId)
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(ByEndDate)
                .ToList();
        }

        public IEnumerable<LotteryDetailsDTO> GetFilteredLotteries(string filter, UserAndOrganizationDTO userOrg)
        {
            return _lotteriesDbSet
                .Where(x => x.OrganizationId == userOrg.OrganizationId)
                .Where(x => x.Title.Contains(filter))
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(ByEndDate);
        }

        public IPagedList<LotteryDetailsDTO> GetPagedLotteries(GetPagedLotteriesArgs args)
        {
            var filteredLotteries = GetFilteredLotteries(args.Filter, args.UserOrg);
            return filteredLotteries.ToPagedList(args.PageNumber, args.PageSize);
        }

        public int GetLotteryStatus(int id)
        {
            return _lotteriesDbSet.Find(id).Status;
        }

        public void EditLotteryStatus(int id, LotteryStatus status)
        {
            var lottery = _lotteriesDbSet.Find(id);

            lottery.Status = (int) status;

            _uow.SaveChanges();
        }

        private Lottery MapNewLottery(CreateLotteryDTO newLotteryDTO)
        {
            var newLottery = _mapper.Map<CreateLotteryDTO, Lottery>(newLotteryDTO);

            newLottery.CreatedBy = newLotteryDTO.UserId;
            newLottery.Modified = DateTime.UtcNow;
            newLottery.ModifiedBy = newLotteryDTO.UserId;

            return newLottery;
        }

        private readonly Expression<Func<LotteryDetailsDTO, DateTime>> ByEndDate = e => e.EndDate;

        private Expression<Func<Lottery, LotteryDetailsDTO>> MapLotteriesToListItemDto =>
         e => new LotteryDetailsDTO
         {
             Id = e.Id,
             Title = e.Title,
             Description = e.Description,
             EndDate = e.EndDate,
             Status = e.Status
         };

        private void UpdateDraftedLottery(Lottery lottery, EditDraftedLotteryDTO draftedLotteryDTO)
        {
            lottery.EntryFee = draftedLotteryDTO.EntryFee;
            lottery.EndDate = draftedLotteryDTO.EndDate;
            lottery.Description = draftedLotteryDTO.Description;
            lottery.Status = draftedLotteryDTO.Status;
            lottery.Title = draftedLotteryDTO.Title;
            lottery.Images = draftedLotteryDTO.Images;
        }
    }
}
