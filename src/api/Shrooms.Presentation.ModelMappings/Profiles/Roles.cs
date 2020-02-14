using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Roles;
using Shrooms.Presentation.WebViewModels.Models.Roles;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Roles : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<RoleDTO, RoleViewModel>();
            CreateMap<RoleUserDTO, RoleUserViewModel>();
            CreateMap<RoleDetailsDTO, RoleDetailsViewModel>();
        }
    }
}
