using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Presentation.WebViewModels.Vacations;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class VacationsProfile : Profile
    {
        public VacationsProfile()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<VacationDto, VacationViewModel>(MemberList.None)
                .ForMember(dest => dest.DateStart, opt => opt.MapFrom(src => src.DateFrom))
                .ForMember(dest => dest.DateEnd, opt => opt.MapFrom(src => src.DateTo));

            CreateMap<VacationAvailableDaysDto, VacationAvailableDaysViewModel>(MemberList.None);
        }
    }
}