using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Presentation.WebViewModels.Models.ExternalLink;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class ExternalLinks : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
            CreateViewModelToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<ExternalLinkDTO, ExternalLinkViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<AddEditDeleteExternalLinkViewModel, AddEditDeleteExternalLinkDTO>()
                .IgnoreUserOrgDto();
            CreateMap<UpdatedExternalLinkViewModel, ExternalLinkDTO>();
            CreateMap<NewExternalLinkViewModel, NewExternalLinkDTO>();
            CreateMap<NewExternalLinkViewModel, ExternalLinkDTO>()
                .Ignore(x => x.Id);
        }
    }
}
