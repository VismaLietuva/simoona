using AutoMapper;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.WebViewModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.ModelMappings.Profiles
{
    public class LotteryProfile : Profile
    {
        protected override void Configure()
        {
            CreateViewModelToDtoMappings();
            CreateDtoMappings();
            CreateLotteryToLotteryDetailDTOMappings();
        }
        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateLotteryDTO, CreateLotteryViewModel>();
            CreateMap<CreateLotteryViewModel, CreateLotteryDTO>();

            CreateMap<LotteryDetailsDTO, LotteryDetailsViewModel>();
            CreateMap<LotteryDetailsViewModel, LotteryDetailsDTO>();

            CreateMap<EditDraftedLotteryViewModel, EditDraftedLotteryDTO>();
            CreateMap<EditDraftedLotteryDTO, EditDraftedLotteryViewModel>();

            CreateMap<EditStartedLotteryViewModel, EditStartedLotteryDTO>();
            CreateMap<EditStartedLotteryDTO, EditStartedLotteryViewModel>();

            CreateMap<BuyLotteryTicketViewModel, BuyLotteryTicketDTO>();
            CreateMap<BuyLotteryTicketDTO, BuyLotteryTicketViewModel>();

            CreateMap<LotteryParticipantViewModel, LotteryParticipantDTO>();
            CreateMap<LotteryParticipantDTO, LotteryParticipantViewModel>();
        }

        private void CreateDtoMappings()
        {
            CreateMap<CreateLotteryDTO, Lottery>()
                .Ignore(x => x.Id);
            CreateMap<Lottery, CreateLotteryDTO>()
                .Ignore(x => x.Id);

            CreateMap<EditDraftedLotteryDTO, Lottery>();
            CreateMap<Lottery, EditDraftedLotteryDTO>();
        }

        public void CreateLotteryToLotteryDetailDTOMappings()
        {
            CreateMap<Lottery, LotteryDetailsDTO>();
            CreateMap<LotteryDetailsDTO, Lottery>();
        }
    }
}
