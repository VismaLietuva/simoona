using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.Presentation.WebViewModels.Models;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Permissions : Profile
    {
        protected override void Configure()
        {
            CreateDtoToViewModelMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<PermissionGroupDto, PermissionGroupViewModel>();
        }
    }
}
