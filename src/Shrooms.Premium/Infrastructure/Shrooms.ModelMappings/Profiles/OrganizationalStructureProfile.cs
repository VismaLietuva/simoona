using AutoMapper;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.OrganizationalStructure;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.OrganizationalStructure;

namespace Shrooms.Premium.Infrastructure.Shrooms.ModelMappings.Profiles
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
