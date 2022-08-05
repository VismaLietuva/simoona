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
            CreateDtoToViewModelMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<LotteryListingArgsViewModel, LotteryListingArgsDto>();
            CreateMap<CreateLotteryViewModel, LotteryDto>();
            CreateMap<LotteryDetailsViewModel, LotteryDetailsDto>();
            CreateMap<EditDraftedLotteryViewModel, LotteryDto>();
            CreateMap<EditStartedLotteryViewModel, EditStartedLotteryDto>();
            CreateMap<BuyLotteryTicketsViewModel, BuyLotteryTicketsDto>();
            CreateMap<LotteryParticipantViewModel, LotteryParticipantDto>();
            CreateMap<LotteryWidgetViewModel, LotteryDetailsDto>();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<LotteryDto, CreateLotteryViewModel>();
            CreateMap<LotteryDetailsDto, LotteryDetailsViewModel>();
            CreateMap<LotteryDto, EditDraftedLotteryViewModel>();
            CreateMap<BuyLotteryTicketsDto, BuyLotteryTicketsViewModel>();
            CreateMap<EditStartedLotteryDto, EditStartedLotteryViewModel>();
            CreateMap<LotteryParticipantDto, LotteryParticipantViewModel>();
            CreateMap<LotteryDetailsDto, LotteryWidgetViewModel>();
            CreateMap<LotteryDetailsBuyerDto, LotteryDetailsBuyerViewModel>();
            CreateMap<LotteryDto, LotteryViewModel>();
        }
    }
}
