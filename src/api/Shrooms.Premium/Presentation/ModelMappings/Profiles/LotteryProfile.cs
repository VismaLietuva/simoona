using AutoMapper;
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
            CreateMap<LotteryTicketReceiverViewModel, LotteryTicketReceiverDto>();
            CreateMap<LotteryListingArgsViewModel, LotteryListingArgsDto>();
            CreateMap<CreateLotteryViewModel, LotteryDto>()
                .Ignore(opt => opt.Id);
            CreateMap<LotteryDetailsViewModel, LotteryDetailsDto>()
                .Ignore(opt => opt.Buyer);
            CreateMap<EditDraftedLotteryViewModel, LotteryDto>();
            CreateMap<EditStartedLotteryViewModel, EditStartedLotteryDto>();
            CreateMap<BuyLotteryTicketsViewModel, BuyLotteryTicketsDto>();
            CreateMap<LotteryDetailsBuyerViewModel, LotteryDetailsBuyerDto>();
            CreateMap<LotteryParticipantViewModel, LotteryParticipantDto>();
            CreateMap<LotteryWidgetViewModel, LotteryDetailsDto>()
                .Ignore(opt => opt.Description)
                .Ignore(opt => opt.Status)
                .Ignore(opt => opt.Images)
                .Ignore(opt => opt.Participants)
                .Ignore(opt => opt.RefundFailed)
                .Ignore(opt => opt.GiftedTicketLimit)
                .Ignore(opt => opt.Buyer);
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<LotteryTicketReceiverDto, LotteryTicketReceiverViewModel>();
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
