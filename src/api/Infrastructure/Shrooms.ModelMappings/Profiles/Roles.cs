using AutoMapper;
using Shrooms.DataTransferObjects.Models.Roles;
using Shrooms.WebViewModels.Models.Roles;

namespace Shrooms.ModelMappings.Profiles
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
