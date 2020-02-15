using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Presentation.WebViewModels.Vacations;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class VacationsProfile : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<VacationDTO, VacationViewModel>()
                .ForMember(dest => dest.DateStart, opt => opt.MapFrom(src => src.DateFrom))
                .ForMember(dest => dest.DateEnd, opt => opt.MapFrom(src => src.DateTo));

            CreateMap<VacationAvailableDaysDTO, VacationAvailableDaysViewModel>();
        }
    }
}