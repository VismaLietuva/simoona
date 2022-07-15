using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Employees;
using Shrooms.Presentation.WebViewModels.Models.Employees;

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
            CreateMap<EmployeeDto, EmployeeViewModel>();
            CreateMap<WorkingHourslWithOutLunchDto, WorkingHourslWithOutLunchViewModel>();
        }
    }
}
