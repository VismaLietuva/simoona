using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.VacationPages;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.WebViewModels.Models.VacationPage;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class VacationPages : Profile
    {
        public VacationPages()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
            CreateEntityToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<VacationPageDto, VacationPageViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<VacationPageViewModel, VacationPageDto>();
        }

        private void CreateEntityToDtoMappings()
        {
            CreateMap<VacationPage, VacationPageDto>();
        }
    }
}