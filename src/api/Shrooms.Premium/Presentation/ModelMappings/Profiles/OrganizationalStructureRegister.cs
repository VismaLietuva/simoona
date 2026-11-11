using Mapster;
using Shrooms.Premium.DataTransferObjects.Models.OrganizationalStructure;
using Shrooms.Premium.Presentation.WebViewModels.OrganizationalStructure;

namespace Shrooms.Premium.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="OrganizationalStructureProfile"/> AutoMapper profile.</summary>
    public class OrganizationalStructureRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Children holds the same type: AutoMapper tracks references on it by itself.
            config.NewConfig<OrganizationalStructureDto, OrganizationalStructureViewModel>()
                .PreserveReference(true);
        }
    }
}
