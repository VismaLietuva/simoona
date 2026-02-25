using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Premium.Presentation.WebViewModels.ServiceRequests;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class ServiceRequestProfile : Profile
    {
        public ServiceRequestProfile()
        {
            CreateViewModelToDtoMappings();
            CreateViewModelMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<ServiceRequestCommentPostViewModel, ServiceRequestCommentDto>(MemberList.None);
            CreateMap<ServiceRequestCreateViewModel, ServiceRequestDto>(MemberList.None)
                .Ignore(x => x.Id)
                .Ignore(x => x.StatusId)
                .Ignore(x => x.CategoryName);
            CreateMap<ServiceRequestUpdateViewModel, ServiceRequestDto>(MemberList.None)
                .Ignore(x => x.CategoryName);

            //Service request category mappings
            CreateMap<ServiceRequestCategoryViewModel, ServiceRequestCategoryDto>(MemberList.None)
                .Ignore(x => x.IsNecessary);
            CreateMap<ServiceRequestCategoryCreateViewModel, ServiceRequestCategoryDto>(MemberList.None)
                .Ignore(x => x.IsNecessary)
                .Ignore(x => x.Id);
        }

        private void CreateViewModelMappings()
        {
            CreateMap<ServiceRequestViewModel, ServiceRequest>(MemberList.None).ReverseMap();
            CreateMap<ServiceRequestPostViewModel, ServiceRequest>(MemberList.None).ReverseMap();

            CreateMap<ServiceRequestComment, ServiceRequestCommentViewModel>(MemberList.None)
               .ForMember(dest => dest.EmployeeFirstName, opt => opt.MapFrom(src => src.Employee.FirstName))
               .ForMember(dest => dest.EmployeeLastName, opt => opt.MapFrom(src => src.Employee.LastName));
        }
    }
}
