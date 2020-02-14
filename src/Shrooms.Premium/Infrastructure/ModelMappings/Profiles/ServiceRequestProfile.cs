using AutoMapper;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.ServiceRequests;

namespace Shrooms.Premium.Infrastructure.ModelMappings.Profiles
{
    public class ServiceRequestProfile : Profile
    {
        protected override void Configure()
        {
            CreateViewModelToDTOMappings();
            CreateViewModelMappings();
        }

        private void CreateViewModelToDTOMappings()
        {
            CreateMap<ServiceRequestCommentPostViewModel, ServiceRequestCommentDTO>();
            CreateMap<ServiceRequestCreateViewModel, ServiceRequestDTO>()
                .Ignore(x => x.Id)
                .Ignore(x => x.StatusId)
                .Ignore(x => x.CategoryName);
            CreateMap<ServiceRequestUpdateViewModel, ServiceRequestDTO>()
                .Ignore(x => x.CategoryName);

            //Service request category mappings
            CreateMap<ServiceRequestCategoryViewModel, ServiceRequestCategoryDTO>()
                .Ignore(x => x.IsNecessary);
            CreateMap<ServiceRequestCategoryCreateViewModel, ServiceRequestCategoryDTO>()
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
