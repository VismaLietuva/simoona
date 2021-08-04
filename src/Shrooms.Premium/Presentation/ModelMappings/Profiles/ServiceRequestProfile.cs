using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Premium.Presentation.WebViewModels.ServiceRequests;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class ServiceRequestProfile : Profile
    {
        protected override void Configure()
        {
            CreateViewModelToDtoMappings();
            CreateViewModelMappings();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<ServiceRequestCommentPostViewModel, ServiceRequestCommentDto>();
            CreateMap<ServiceRequestCreateViewModel, ServiceRequestDto>()
                .Ignore(x => x.Id)
                .Ignore(x => x.StatusId)
                .Ignore(x => x.CategoryName);
            CreateMap<ServiceRequestUpdateViewModel, ServiceRequestDto>()
                .Ignore(x => x.CategoryName);

            //Service request category mappings
            CreateMap<ServiceRequestCategoryViewModel, ServiceRequestCategoryDto>()
                .Ignore(x => x.IsNecessary);
            CreateMap<ServiceRequestCategoryCreateViewModel, ServiceRequestCategoryDto>()
                .Ignore(x => x.IsNecessary)
                .Ignore(x => x.Id);
        }

        private void CreateViewModelMappings()
        {
            CreateMap<ServiceRequestViewModel, ServiceRequest>().ReverseMap();
            CreateMap<ServiceRequestPostViewModel, ServiceRequest>().ReverseMap();

            CreateMap<ServiceRequestComment, ServiceRequestCommentViewModel>()
               .ForMember(dest => dest.EmployeeFirstName, opt => opt.MapFrom(src => src.Employee.FirstName))
               .ForMember(dest => dest.EmployeeLastName, opt => opt.MapFrom(src => src.Employee.LastName));
        }
    }
}
