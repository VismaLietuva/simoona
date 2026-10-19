using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Presentation.WebViewModels.Models.ExternalLink;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="ExternalLinks"/> AutoMapper profile.</summary>
    public class ExternalLinksRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            CreateDtoToViewModelMappings(config);
            CreateViewModelToDtoMappings(config);
        }

        private static void CreateDtoToViewModelMappings(TypeAdapterConfig config)
        {
            config.NewConfig<ExternalLinkDto, ExternalLinkViewModel>();
        }

        private static void CreateViewModelToDtoMappings(TypeAdapterConfig config)
        {
            config.NewConfig<ManageExternalLinkViewModel, ManageExternalLinkDto>()
                .IgnoreUserOrgDto();
            config.NewConfig<UpdatedExternalLinkViewModel, ExternalLinkDto>();
            config.NewConfig<NewExternalLinkViewModel, NewExternalLinkDto>();
            // Not translated: the profile's Ignore(Id). NewExternalLinkViewModel has no Id, so it changes nothing here, but
            // Mapster applies a base source type's config to derived ones (implicit source inheritance) and an inherited
            // Ignore cannot be overridden: it would drop Id from UpdatedExternalLinkViewModel -> ExternalLinkDto above.
            config.NewConfig<NewExternalLinkViewModel, ExternalLinkDto>();
        }
    }
}
