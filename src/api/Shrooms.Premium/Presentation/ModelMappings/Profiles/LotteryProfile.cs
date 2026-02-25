using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Presentation.WebViewModels.Lotteries;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class LotteryProfile : Profile
    {
        public LotteryProfile()
        {
            CreateViewModelToDtoMappings();
            CreateDtoToViewModelMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<LotteryTicketReceiverViewModel, LotteryTicketReceiverDto>(MemberList.None);
            CreateMap<LotteryListingArgsViewModel, LotteryListingArgsDto>(MemberList.None);
            CreateMap<CreateLotteryViewModel, LotteryDto>(MemberList.None)
                .Ignore(opt => opt.Id);
            CreateMap<LotteryDetailsViewModel, LotteryDetailsDto>(MemberList.None)
                .Ignore(opt => opt.Buyer);
            CreateMap<EditDraftedLotteryViewModel, LotteryDto>(MemberList.None);
            CreateMap<EditStartedLotteryViewModel, EditStartedLotteryDto>(MemberList.None);
            CreateMap<BuyLotteryTicketsViewModel, BuyLotteryTicketsDto>(MemberList.None);
            CreateMap<LotteryDetailsBuyerViewModel, LotteryDetailsBuyerDto>(MemberList.None);
            CreateMap<LotteryParticipantViewModel, LotteryParticipantDto>(MemberList.None);
            CreateMap<LotteryWidgetViewModel, LotteryDetailsDto>(MemberList.None)
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
            CreateMap<LotteryTicketReceiverDto, LotteryTicketReceiverViewModel>(MemberList.None);
            CreateMap<LotteryDto, CreateLotteryViewModel>(MemberList.None);
            CreateMap<LotteryDetailsDto, LotteryDetailsViewModel>(MemberList.None);
            CreateMap<LotteryDto, EditDraftedLotteryViewModel>(MemberList.None);
            CreateMap<BuyLotteryTicketsDto, BuyLotteryTicketsViewModel>(MemberList.None);
            CreateMap<EditStartedLotteryDto, EditStartedLotteryViewModel>(MemberList.None);
            CreateMap<LotteryParticipantDto, LotteryParticipantViewModel>(MemberList.None);
            CreateMap<LotteryDetailsDto, LotteryWidgetViewModel>(MemberList.None);
            CreateMap<LotteryDetailsBuyerDto, LotteryDetailsBuyerViewModel>(MemberList.None);
            CreateMap<LotteryDto, LotteryViewModel>(MemberList.None);
        }
    }
}
