using AutoMapper;
using Shrooms.DataTransferObjects.Models.OrganizationalStructure;
using Shrooms.WebViewModels.Models.OrganizationalStructure;

namespace Shrooms.ModelMappings.Profiles
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
