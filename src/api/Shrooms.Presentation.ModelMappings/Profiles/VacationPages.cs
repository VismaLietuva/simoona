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
            CreateMap<VacationPageDto, VacationPageViewModel>(MemberList.None);
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<VacationPageViewModel, VacationPageDto>(MemberList.None);
        }

        private void CreateEntityToDtoMappings()
        {
            CreateMap<VacationPage, VacationPageDto>(MemberList.None);
        }
    }
}