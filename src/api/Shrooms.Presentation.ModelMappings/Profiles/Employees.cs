using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Employees;
using Shrooms.Presentation.WebViewModels.Models.Employees;
using System;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Employees : Profile
    {
        protected override void Configure()
        {
            CreateViewModelToDtoMappings();
            CreateDtoToViewModelMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<EmployeeListingArgsViewModel, EmployeeListingArgsDto>();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<EmployeeDto, EmployeeViewModel>()
                .ForMember(dest => dest.BlacklistEndDate, opt => opt.MapFrom(u => u.BlacklistEntry.EndDate));
            CreateMap<WorkingHourslWithOutLunchDto, WorkingHourslWithOutLunchViewModel>();
        }
    }
}
