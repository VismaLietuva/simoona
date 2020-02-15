using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.OrganizationalStructure;
using Shrooms.Premium.Presentation.WebViewModels.OrganizationalStructure;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class OrganizationalStructureProfile : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<OrganizationalStructureDTO, OrganizationalStructureViewModel>();
        }
    }
}
