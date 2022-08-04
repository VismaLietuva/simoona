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
            CreateLotteryToLotteryDetailDtoMappings();
        }
        private void CreateViewModelToDtoMappings()
        {
            CreateMap<LotteryDto, CreateLotteryViewModel>();
            CreateMap<CreateLotteryViewModel, LotteryDto>();

            CreateMap<LotteryDetailsDto, LotteryDetailsViewModel>();
            CreateMap<LotteryDetailsViewModel, LotteryDetailsDto>();

            CreateMap<EditDraftedLotteryViewModel, LotteryDto>();
            CreateMap<LotteryDto, EditDraftedLotteryViewModel>();

            CreateMap<EditStartedLotteryViewModel, EditStartedLotteryDto>();
            CreateMap<EditStartedLotteryDto, EditStartedLotteryViewModel>();

            CreateMap<BuyLotteryTicketsViewModel, BuyLotteryTicketsDto>();
            CreateMap<BuyLotteryTicketsDto, BuyLotteryTicketsViewModel>();

            CreateMap<LotteryParticipantViewModel, LotteryParticipantDto>();
            CreateMap<LotteryParticipantDto, LotteryParticipantViewModel>();
            CreateMap<LotteryWidgetViewModel, LotteryDetailsDto>();
            CreateMap<LotteryDetailsDto, LotteryWidgetViewModel>();
        }

        private void CreateDtoMappings()
        {
            CreateMap<LotteryDto, Lottery>()
                .Ignore(x => x.Id);
            CreateMap<Lottery, LotteryDto>()
                .Ignore(x => x.Id);

            CreateMap<LotteryDto, Lottery>();
            CreateMap<Lottery, LotteryDto>();
        }

        private void CreateLotteryToLotteryDetailDtoMappings()
        {
            CreateMap<Lottery, LotteryDetailsDto>();
            CreateMap<LotteryDetailsDto, Lottery>();
        }
    }
}
