using AutoMapper;
using Shrooms.DataTransferObjects.Models.ExternalLinks;
using Shrooms.WebViewModels.Models.ExternalLink;

namespace Shrooms.ModelMappings.Profiles
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
