using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;

namespace Shrooms.Domain.Services.Lotteries
{
    public class LotteryService : ILotteryService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Lottery> _lotteriesDbSet;
        private readonly IMapper _mapper;

        public LotteryService(IUnitOfWork2 uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _lotteriesDbSet = uow.GetDbSet<Lottery>();
        }
        public async Task<CreateLotteryDTO> CreateLottery(CreateLotteryDTO newLotteryDTO)
        {
            var newLottery = MapNewLottery(newLotteryDTO);
            _lotteriesDbSet.Add(newLottery);
            await _uow.SaveChangesAsync(newLotteryDTO.UserId);

            newLotteryDTO.Id = newLottery.Id.ToString();

            return newLotteryDTO;
        }

        public void EditDraftedLottery(EditDraftedLotteryDTO lotteryDTO)
        {
            throw new NotImplementedException();
        }

        public void EditStartedLottery(EditStartedLotteryDTO lotteryDTO)
        {
            throw new NotImplementedException();
        }

        public LotteryDetailsDTO GetLotteryDetails(Guid id, UserAndOrganizationDTO userOrg)
        {
            throw new NotImplementedException();
        }

        public void RemoveLottery(int id, UserAndOrganizationDTO userOrg)
        {
            var @lottery = _lotteriesDbSet.SingleOrDefault(p => p.Id == id && p.OrganizationId == userOrg.OrganizationId);
            @lottery.Status = 2;

         //   _lotteriesDbSet.Remove(@lottery);

            _uow.SaveChanges(false);


        }
        public IEnumerable<LotteryDetailsDTO> GetLotteries(UserAndOrganizationDTO userOrganization)
        {
            var lotteries = _lotteriesDbSet
                .Where(p => p.OrganizationId == userOrganization.OrganizationId)
                .Select(MapLotteriesToListItemDto())
                .OrderBy(p => p.EndDate).ToList();

            return lotteries;
        }
        private Lottery MapNewLottery(CreateLotteryDTO newLotteryDTO)
        {
            var newLottery = _mapper.Map<CreateLotteryDTO, Lottery>(newLotteryDTO);

            newLottery.Created = newLotteryDTO.StartDate;
            newLottery.CreatedBy = newLotteryDTO.UserId;
            newLottery.Images = new ImagesCollection { "Images" };
            newLottery.Modified = DateTime.UtcNow;
            newLottery.ModifiedBy = newLotteryDTO.UserId;

            return newLottery;
        }
        private Expression<Func<Lottery, LotteryDetailsDTO>> MapLotteriesToListItemDto()
        {
            return e => new LotteryDetailsDTO
            {
               Id = e.Id,
               Title = e.Title,
               Description = e.Description,
               EndDate = e.EndDate,
               Status = e.Status
            };
        }
    }
}
