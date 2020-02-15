using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Presentation.WebViewModels.Lotteries;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
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
            CreateMap<LotteryDTO, CreateLotteryViewModel>();
            CreateMap<CreateLotteryViewModel, LotteryDTO>();

            CreateMap<LotteryDetailsDTO, LotteryDetailsViewModel>();
            CreateMap<LotteryDetailsViewModel, LotteryDetailsDTO>();

            CreateMap<EditDraftedLotteryViewModel, LotteryDTO>();
            CreateMap<LotteryDTO, EditDraftedLotteryViewModel>();

            CreateMap<EditStartedLotteryViewModel, EditStartedLotteryDTO>();
            CreateMap<EditStartedLotteryDTO, EditStartedLotteryViewModel>();

            CreateMap<BuyLotteryTicketViewModel, BuyLotteryTicketDTO>();
            CreateMap<BuyLotteryTicketDTO, BuyLotteryTicketViewModel>();

            CreateMap<LotteryParticipantViewModel, LotteryParticipantDTO>();
            CreateMap<LotteryParticipantDTO, LotteryParticipantViewModel>();
            CreateMap<LotteryWidgetViewModel, LotteryDetailsDTO>();
            CreateMap<LotteryDetailsDTO, LotteryWidgetViewModel>();
        }

        private void CreateDtoMappings()
        {
            CreateMap<LotteryDTO, Lottery>()
                .Ignore(x => x.Id);
            CreateMap<Lottery, LotteryDTO>()
                .Ignore(x => x.Id);

            CreateMap<LotteryDTO, Lottery>();
            CreateMap<Lottery, LotteryDTO>();
        }

        public void CreateLotteryToLotteryDetailDTOMappings()
        {
            CreateMap<Lottery, LotteryDetailsDTO>();
            CreateMap<LotteryDetailsDTO, Lottery>();
        }
    }
}
