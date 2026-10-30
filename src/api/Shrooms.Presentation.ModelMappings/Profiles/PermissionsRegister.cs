using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.Presentation.WebViewModels.Models;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Permissions"/> AutoMapper profile.</summary>
    public class PermissionsRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PermissionGroupDto, PermissionGroupViewModel>();
        }
    }
}
