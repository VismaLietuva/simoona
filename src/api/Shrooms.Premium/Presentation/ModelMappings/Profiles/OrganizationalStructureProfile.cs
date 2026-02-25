using AutoMapper;
using Shrooms.Premium.DataTransferObjects.Models.OrganizationalStructure;
using Shrooms.Premium.Presentation.WebViewModels.OrganizationalStructure;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    public class OrganizationalStructureProfile : Profile
    {
        public OrganizationalStructureProfile()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<OrganizationalStructureDto, OrganizationalStructureViewModel>();
        }
    }
}
