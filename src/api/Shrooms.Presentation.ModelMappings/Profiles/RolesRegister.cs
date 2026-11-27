using Mapster;
using Shrooms.Contracts.DataTransferObjects.Models.Roles;
using Shrooms.Presentation.WebViewModels.Models.Roles;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    /// <summary>Mapster translation of the <see cref="Roles"/> AutoMapper profile.</summary>
    public class RolesRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RoleDto, RoleViewModel>();
            config.NewConfig<RoleUserDto, RoleUserViewModel>();
            config.NewConfig<RoleDetailsDto, RoleDetailsViewModel>();
        }
    }
}
