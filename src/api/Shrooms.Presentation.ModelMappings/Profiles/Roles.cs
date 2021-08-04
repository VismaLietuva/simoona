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
            CreateMap<RoleDto, RoleViewModel>();
            CreateMap<RoleUserDto, RoleUserViewModel>();
            CreateMap<RoleDetailsDto, RoleDetailsViewModel>();
        }
    }
}
