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
            CreateMap<ExternalLinkDto, ExternalLinkViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<AddEditDeleteExternalLinkViewModel, AddEditDeleteExternalLinkDto>()
                .IgnoreUserOrgDto();
            CreateMap<UpdatedExternalLinkViewModel, ExternalLinkDto>();
            CreateMap<NewExternalLinkViewModel, NewExternalLinkDto>();
            CreateMap<NewExternalLinkViewModel, ExternalLinkDto>()
                .Ignore(x => x.Id);
        }
    }
}
