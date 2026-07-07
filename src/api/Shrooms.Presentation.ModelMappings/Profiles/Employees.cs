using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Employees;
using Shrooms.Presentation.WebViewModels.Models.Employees;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Employees : Profile
    {
        public Employees()
        {
            CreateViewModelToDtoMappings();
            CreateDtoToViewModelMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<EmployeeListingArgsViewModel, EmployeeListingArgsDto>(MemberList.None);
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<EmployeeDto, EmployeeViewModel>(MemberList.None)
                .ForMember(dest => dest.BlacklistEndDate, opt => opt.MapFrom(u => u.BlacklistEntry.EndDate));
            CreateMap<WorkingHourslWithOutLunchDto, WorkingHourslWithOutLunchViewModel>(MemberList.None);
        }
    }
}
